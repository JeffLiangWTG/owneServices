using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using SupportingDocument = Enterprise.Customs.GB.Business.Declaration.SupportingDocument;

namespace Enterprise.Customs.GB.Business.Messaging
{
	public abstract class GbLine : ILine
	{
		protected GbLine(GbHeader header, Declaration.CusEntryLine actualEntryLine)
		{
			Argument.NotNull(actualEntryLine, nameof(actualEntryLine));

			this.GbHeader = header;
			this.actualEntryLine = actualEntryLine;
			this.entryLine = actualEntryLine;
			this.randomLine = actualEntryLine.RandomLine;
		}
		readonly ICusEntryLine entryLine;
		public readonly GbHeader GbHeader;

		public readonly Declaration.CusEntryLine actualEntryLine;    // to be removed when ICusEntryLine is implemented in GB
		protected readonly Declaration.JobComInvoiceLine randomLine;

		ZString ILine.DeclarationType
		{
			get { return GbHeader.DeclarationType; } //HACK: different decl type on header and line
		}

		ZString ILine.CommodityCode
		{
			get { return entryLine.Tariff; }
		}

		OrgAddress GetItemLevelPartyOrThrowIfDefinedTwice(OrgAddress lineParty, OrgAddress declarationParty)
		{
			if (lineParty != null && declarationParty == null)
			{
				// Bulk entry - party at invoice level, declaration party is blank
				return lineParty;
			}

			if (lineParty != null && declarationParty != null)
			{
				if (lineParty.PK != declarationParty.PK)
				{
					// Defined in both places and they're different.
					// NB this exception should never be thrown because the message creators shoud be smart enough to realise they've already added box 2/box 8 parties and shoudl not even try to add box 2i and box 8i parties.  But just in case....
					throw new NotSupportedException("You can't have an invoice-level party that differs from the declaration-level party. Remove the declaration party (" + declarationParty.Header.OH_FullName + ")");
				}
				else
				{
					// Don't put anything in the item-level fields.  Rely on the header-level parties. 
					return null;
				}
			}
			return null;
		}

		// for item-level parties...
		public OrgAddress Consignee => GetItemLevelPartyOrThrowIfDefinedTwice(actualEntryLine.Consignee, actualEntryLine.Declaration.ImporterDocumentaryAddress.Address);

		// for item-level parties...
		public OrgAddress Shipper => GetItemLevelPartyOrThrowIfDefinedTwice(actualEntryLine.Consignor, actualEntryLine.Declaration.SupplierDocumentaryAddress.Address);

		IOrganisation ILine.Shipper
		{
			get
			{
				return OrganisationProvider.Get(this.Shipper);
			}
		}

		IOrganisation ILine.Consignee
		{
			get
			{
				return OrganisationProvider.Get(this.Consignee);
			}
		}

		IEnumerable<IContainer> ILine.Containers
		{
			get
			{
				Dictionary<ZString, IContainer> containers = new Dictionary<ZString, IContainer>();
				foreach (JobComInvoiceLine invoiceLine in actualEntryLine.InvoiceLines)
				{
					foreach (NonPersistentCusContainer container in invoiceLine.ContainersForInvoiceLinesForBindingOnly)
					{
						if (!container.ContainerNumber.IsEmpty && container.IsForInvoiceLine && !containers.ContainsKey(container.ContainerNumber))
						{
							containers[container.ContainerNumber] = new Container(container);
						}
					}
				}
				return containers.Values;
			}
		}

		ZString ILine.CountryOriginCode
		{
			get { return GetCountryOfOriginBox34(); }
		}

		protected virtual ZString GetCountryOfOriginBox34()
		{
			return randomLine.JI_CountryOfOrigin;
		}

		ZString ILine.DescriptionOfGoods
		{
			get { return entryLine.Description; }
		}

		ZDecimal ILine.GrossMassInKilograms
		{
			get
			{
				return actualEntryLine.EffectiveGrossWeightIsApplicable
					? GetGrossWeightOfEntryLineInKilosWithoutLookingAtInvoiceHeaderOrDeclaration() : ZDecimal.Zero;
			}
		}

		ZDecimal GetGrossWeightOfEntryLineInKilosWithoutLookingAtInvoiceHeaderOrDeclaration()
		{
			var result = ZWeight.Empty;
			foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
			{
				var potentialWeight = new ZWeight(invoiceLine.JI_Weight, invoiceLine.JI_WeightUQ);
				if (potentialWeight.IsValid)
				{
					result += potentialWeight;
				}
			}
			return result.InKilograms;
		}

		ZDecimal ILine.NetMassInKilograms
		{
			get { return new ZWeight(actualEntryLine.CustomsQuantity, randomLine.CustomsQuantityConverter.GetEffectiveWeightUnit(actualEntryLine.CustomsUnitQty)).InKilogramsSafe; }
		}

		IEnumerable<ISupportingDocument> ILine.SupportingDocuments
		{
			get
			{
				return actualEntryLine.SupportingDocuments.Select(x => new Document((SupportingDocument)x));
			}
		}

		IEnumerable<IPreviousDocument> ILine.PreviousDocuments
		{
			get
			{
				return actualEntryLine.PreviousDocuments.Select(x => new PreviousDocumentWrapper(x));
			}
		}

		IEnumerable<IStatement> ILine.Statements
		{
			get
			{
				return actualEntryLine.AdditionalInfos.Select(x => new Statement(x));
			}
		}

		IEnumerable<IPackage> ILine.Packages
		{
			get
			{
				Dictionary<MergeKey, Package> packages = new Dictionary<MergeKey, Package>();
				foreach (JobComInvoiceLine invoiceLine in actualEntryLine.InvoiceLines)
				{
					foreach (InvoiceLinePackagePivot pivot in invoiceLine.PackagesPivot)
					{
						MergeKey mergeKey = new MergeKey();
						mergeKey.Add(pivot.Package.CW_PackType);
						mergeKey.Add(pivot.Package.CW_MarksAndNos);
						Package package;
						if (!packages.TryGetValue(mergeKey, out package))
						{
							package = new Package();
							packages[mergeKey] = package;
						}
						package.Add(pivot);
					}
				}
				return new TypedEnumerable<IPackage>(packages.Values);
			}
		}

		ZString ILine.PrincipalsRepresentativeCity
		{
			get { return randomLine.ZG_RL_NKPrincipalsRepresentativeCity; }
		}

		ZString ILine.PrincipalsRepresentativeName
		{
			get { return randomLine.ZG_PrincipalsRepresentativeName; }
		}

		ZString ILine.Procedure
		{
			get { return randomLine.JI_Procedure; }
		}

		ZDecimal ILine.StatisticalValue
		{
			get { return actualEntryLine.StatisticalValue; }
		}

		IOrganisation ILine.SupervisingOffice
		{
			get { return OrganisationProvider.Get(randomLine.SupervisingOffice); }
		}

		ZString ILine.SupplementaryCode1
		{
			get { return randomLine.JI_SupplementaryCode1; }
		}

		ZString ILine.SupplementaryCode2
		{
			get { return randomLine.JI_SupplementaryCode2; }
		}

		ZDecimal ILine.SupplementaryUnits
		{
			get { return actualEntryLine.SupplementaryQuantity; }
		}

		ZString ILine.SupplementaryUnitsUQ
		{
			get { return actualEntryLine.SupplementaryUQ; }
		}

		IEnumerable<ITax> ILine.Taxes
		{
			get
			{
				List<ITax> result = new List<ITax>();
				foreach (TaxStruct tax in actualEntryLine.Taxes)
				{
					result.Add(new Tax(tax));
				}
				return result;
			}
		}

		ZDecimal ILine.ThirdQuantity
		{
			get { return actualEntryLine.ThirdQuantity; }
		}

		ZString ILine.UNDGCode
		{
			get { return randomLine.UNDGs.Count > 0 && randomLine.UNDGs[0].UNDGSubstance is UNDGSubstance substance ? substance.DG_Code : ZString.Empty; }
		}

		ZBool ILine.FECNetMassInKilograms
		{
			get { return randomLine.JI_FecQV1_NettMass; }
		}

		ZBool ILine.FECSupplementaryUnits
		{
			// Check this should be units not qty? - It's OK, the mesage contains only the qty
			get { return randomLine.JI_FecQV2_Supp; }
		}

		ZBool ILine.IsProcedureThatAllowsZeroSupplementaryQty
		{
			get { return randomLine.CusProcedure?.HasAttribute(Universal.AttributeNames.Codes.ZeroSupplementaryQty) ?? false; }
		}
	}
}
