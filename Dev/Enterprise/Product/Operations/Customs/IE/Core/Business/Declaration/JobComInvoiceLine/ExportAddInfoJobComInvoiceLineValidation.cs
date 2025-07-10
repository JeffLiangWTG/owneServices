using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		public ExportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		bool isValidateAll;

		public override void ValidateAll()
		{
			isValidateAll = true;
			base.ValidateAll();
			isValidateAll = false;
		}

		protected override void CheckZG_IsMainPack()
		{
			var parent = Parent;
			if (parent.OverallPackageType == PackageType.Packed)
			{
				if (isValidateAll)
				{
					if (parent.Declaration is JobDeclaration declaration && (declaration.OneMainPackInvoiceLinePerPackageValidationResult?.TryGetValue(parent.PK, out var errorMessage) ?? false))
					{
						parent.ZG_IsMainPackInfo.AddMessageError(errorMessage);
					}
				}
				else
				{
					var packagePKs = parent.PackagesPivot.Cast<InvoiceLinePackagePivot>().Select(x => x.CHC_CW).ToHashSet();
					if (packagePKs.Any() && parent.EntryInstruction is CusEntryInstruction instruction)
					{
						var invoiceLinePK = parent.PK;
						if (parent.ZG_IsMainPack)
						{
							var otherMainPackInvoiceLines = instruction.MainPackInvoiceLines.Where(line => line.PK != invoiceLinePK && line.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(p => packagePKs.Contains(p.CHC_CW)));
							if (otherMainPackInvoiceLines.Any())
							{
								parent.ZG_IsMainPackInfo.AddMessageError(GetOneMainPackInvoiceLinePerPackageMesssageMulti(otherMainPackInvoiceLines.Select(x => x.InvoiceAndLineReference).Prepend(parent.InvoiceAndLineReference).ToArray()));
							}
						}
						else
						{
							var commonPackLines = instruction.InvoiceLines.Cast<JobComInvoiceLine>().Where(line => line.PK != invoiceLinePK && line.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(p => packagePKs.Contains(p.CHC_CW)));
							if (commonPackLines.Any() && !commonPackLines.Any(x => x.ZG_IsMainPack))
							{
								parent.ZG_IsMainPackInfo.AddMessageError(GetOneMainPackInvoiceLinePerPackageMesssageNone(commonPackLines.Select(x => x.InvoiceAndLineReference).Prepend(parent.InvoiceAndLineReference).ToArray()));
							}
						}
					}
				}
			}
			else
			{
				if (parent.ZG_IsMainPack && parent.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(x => x.Package is Package package && (package.PackageType == PackageType.Bulk || package.PackageType == PackageType.BreakBulk)))
				{
					parent.ZG_IsMainPackInfo.AddWarning(Res.GetString("61E988EB-4CAD-46EF-89E0-43E63479E35F", "When Invoice Line is bulk or break bulk, then Is Main Pack does not need to be indicated and has no effect."));
				}
			}
		}

		public static string GetOneMainPackInvoiceLinePerPackageMesssageNone(ZString[] invoiceAndLineReferences) => Res.GetString("61E988EB-4CAD-46EF-89E0-43E63479E35E", "Where Invoice Lines are Packed AND the Linked Packages are Common then one and only one of the Invoice Lines must be indicated as Is Main Pack. One of {0} should be indicated as Is Main Pack.", ZString.Join(", ", invoiceAndLineReferences));

		public static string GetOneMainPackInvoiceLinePerPackageMesssageMulti(ZString[] invoiceAndLineReferences) => Res.GetString("61E988EB-4CAD-46EF-89E0-43E63479E351", "Where Invoice Lines are Packed AND the Linked Packages are Common then one and only one of the Invoice Lines must be indicated as Is Main Pack. Only one of {0} should be indicated as Is Main Pack.", ZString.Join(", ", invoiceAndLineReferences));

		protected override void CheckZG_CountryOfDestination()
		{
			var parent = Parent;
			var countryOfDestination = parent.ZG_CountryOfDestination;
			var targetInfo = parent.ZG_CountryOfDestinationInfo;
			if (countryOfDestination.IsEmpty)
			{
				MandatoryValidation.AddYouHaveNotEnteredMessage(targetInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);

				if (parent.Declaration is JobDeclaration declaration)
				{
					CheckZG_CountryOfDestination_B1872_B1871(declaration, countryOfDestination);

					var entryStyle = declaration.JE_EntryStyle.ToUpperInvariant();
					switch (entryStyle)
					{
						case MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion:
							if (RefCountryHelper.IsCountryPartOfEuropeanUnion(parent.Factory, countryOfDestination))
							{
								targetInfo.AddMessageError(Res.GetString("3E0856EA-85E0-49C6-9A1C-455E8DCE5E1B", "Destination country should NOT be an EU country, when declaration type is '{0}'.", EU.Business.EntryStyleListExport.Codes.ExportNormal));
							}
							break;
						case MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec:
							if (!RefCountryHelper.IsCountryPartOfEuropeanUnion(parent.Factory, countryOfDestination))
							{
								targetInfo.AddMessageError(Res.GetString("B8317023-BDCA-458A-BF15-CDAE2549A276", "Destination country should be an EU country, when declaration type is '{0}'.", EU.Business.EntryStyleListExport.Codes.ExportToSpecialTerritory));
							}
							break;
					}
				}
			}
		}

		void CheckZG_CountryOfDestination_B1872_B1871(JobDeclaration declaration, ZString countryOfDestination)
		{
			if (declaration.IsTransitionPeriodAES30)
			{
				var parent = Parent;
				var lookups = (ExportAddInfoJobComInvoiceLineLookups)parent.AddInfoLookups;
				var cl140List = lookups.CountryOfDestinationCL140;
				if (!cl140List.ContainsCode(countryOfDestination) && declaration.FilteredInvoiceLines.Any(x => x != parent && cl140List.ContainsCode(x.ZG_CountryOfDestination)))
				{
					Parent.ZG_CountryOfDestinationInfo.AddMessageError(Res.GetString("ca8d7407-045a-428d-826d-b06e97f74e45", "If one Invoice Line has Country of Destination within Other Regime Country Codes List (CL140) or 'AD' , 'SM' ,'DE' , 'IT' Excluding  'QQ' , 'QR' , 'QV', then all lines should have a Country of Destination within the same list."));
				}

				var cl063List = lookups.CountryOfDestinationCL063;
				if (!cl063List.ContainsCode(countryOfDestination) && declaration.FilteredInvoiceLines.Any(x => x != parent && cl063List.ContainsCode(x.ZG_CountryOfDestination)))
				{
					Parent.ZG_CountryOfDestinationInfo.AddMessageError(Res.GetString("0cdd5f35-87ec-49d7-8456-8b9fbf24d28e", "If one Invoice Line has Country of Destination within Common Transit Community Country Codes List (CL063) excluding 'AD', 'SM', then all lines should have a Country of Destination within the same list."));
				}
			}
		}
	}
}
