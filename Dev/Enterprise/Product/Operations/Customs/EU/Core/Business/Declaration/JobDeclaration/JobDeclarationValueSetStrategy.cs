using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobDeclarationValueSetStrategy : IValueSetStrategy
	{
		public JobDeclarationValueSetStrategy(JobDeclaration declaration)
		{
			Declaration = declaration;
		}

		protected readonly JobDeclaration Declaration;

		#region IValueSetStrategy Members

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			ValueSetCore(valueThatHasChanged, oldValue);
		}

		#endregion

		#region Default Values

		protected virtual void DefaultCT_Status(ZPropertyInfo valueThatHasChanged)
		{
		}

		protected virtual void DefaultImporterChanged()
		{
			var importer = Declaration.Importer;

			if (Declaration.IsImport)
			{
				SetBox14Representation(importer);
			}

			if (importer != null)
			{
				SetDefaultFiscalReferences(true);
			}

			Declaration.VATDeferStrategy.OnOrganisationChanged();
		}

		protected virtual void DefaultDutyPayerChanged()
		{
		}

		protected virtual void DefaultDeclarantChanged()
		{
			Declaration.VATDeferStrategy.OnOrganisationChanged();
		}

		protected virtual void DefaultRepresentativeChanged()
		{
		}

		protected virtual void DefaultSupplierChanged()
		{
			if (Declaration.IsExport)
			{
				SetBox14Representation(Declaration.Supplier);
			}
		}

		protected virtual void DefaultPaymentMethodChanged()
		{
			Declaration.VATDeferStrategy.OnPaymentMethodChanged();
		}

		protected virtual ZString GetNewCtStatusId()
		{
			return ZString.Empty;
		}

		protected virtual void BranchChangedHandler()
		{
			// country-specific implementation do this
		}

		#endregion
		protected virtual void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			DefaultCT_Status(valueThatHasChanged);
			switch (valueThatHasChanged.Name)
			{
				case JobDeclaration.Schema.JE_RL_NKOrigin:
				case JobDeclaration.Schema.JE_RL_NKFinalDestination:
					Declaration.ZG_CTStatusID = GetNewCtStatusId();
					break;
				case JobDeclaration.Schema.JE_GB:
					BranchChangedHandler();
					break;
				case JobDeclaration.Schema.JE_OH_Importer:
					DefaultImporterChanged();
					break;
				case JobDeclaration.Schema.JE_OH_Supplier:
					DefaultSupplierChanged();
					break;
				case JobDeclaration.Schema.JE_OA_Representative:
					DefaultRepresentativeChanged();
					break;
				case JobDeclaration.Schema.JE_OA_DeclarantAddress:
					DefaultDeclarantChanged();
					break;
				case JobDeclaration.Schema.JE_PaymentMethod:
					DefaultPaymentMethodChanged();
					break;
				case JobDeclaration.Schema.JE_MessageType:
					var orgHeader = Declaration.IsImport ? Declaration.Importer : Declaration.Supplier;
					SetBox14Representation(orgHeader);
					break;
				case JobDeclaration.Schema.JE_VATDeferType:
					VATDeferTypeChanged();
					break;
				case JobDeclaration.Schema.JE_OH_DutyPayer:
					DefaultDutyPayerChanged();
					break;
			}
		}

		protected virtual void VATDeferTypeChanged()
		{
			Declaration.VATDeferStrategy.OnVATDeferTypeChanged();
		}

		public void SetImportEntrySubStyle() => SetImportEntrySubStyleCore();
		protected virtual void SetImportEntrySubStyleCore() { }

		public void DefaultEntrySubStyle(CusEntryInstruction cei) => DefaultEntrySubStyleCore(cei);

		protected virtual void DefaultEntrySubStyleCore(CusEntryInstruction cei) { }

		public void DefaultEntryStyle(CusEntryInstruction cei) => DefaultEntryStyleCore(cei);

		protected virtual void DefaultEntryStyleCore(CusEntryInstruction cei) { }

		protected FiscalRepresentativeDefaulter fiscalRepresentativeDefaulter;
		public FiscalRepresentativeDefaulter FiscalRepresentativeDefaulter
		{
			get
			{
				if (fiscalRepresentativeDefaulter == null)
				{
					DefaultFiscalRepresentativeDefaulter();
				}
				return fiscalRepresentativeDefaulter;
			}
		}
		public void DefaultFiscalRepresentativeDefaulter() => DefaultFiscalRepresentativeDefaulterCore();
		protected virtual void DefaultFiscalRepresentativeDefaulterCore() => fiscalRepresentativeDefaulter = new FiscalRepresentativeDefaulter();

		protected void SetBox14Representation(OrgHeader orgHeader)
		{
			var declarantType = GetDeclarantType(orgHeader);
			if (!declarantType.IsEmpty)
			{
				Declaration.JE_DeclarantType = declarantType;
			}
		}

		protected virtual ZString GetDeclarantType(OrgHeader orgHeader)
		{
			var result = ZString.Empty;
			if (orgHeader != null)
			{
				var addInfo = EUOrgImpAddInfo.Get(orgHeader, Declaration.CountryCode);
				if (addInfo != null)
				{
					addInfo.Deserialise();
					if (Declaration.IsExport)
					{
						if (addInfo.ZO_Box14UseIndirectRepresentationForExporter)
						{
							result = RepresentationTypeList.Codes._3Indirect;
						}
					}
					else if (Declaration.IsImport)
					{
						result = addInfo.ZO_Box14UseIndirectRepresentation ? RepresentationTypeList.Codes._3Indirect : RepresentationTypeList.Codes._2Direct;
					}
				}
			}
			return result;
		}

		public void SetDefaultFiscalReferences(ZBool usePostponedVatAccounting) => SetDefaultFiscalReferencesCore(usePostponedVatAccounting);

		protected virtual void SetDefaultFiscalReferencesCore(ZBool usePostponedVatAccounting)
		{
			if (!usePostponedVatAccounting)
			{
				return;
			}

			foreach (CusEntryInstruction entryinstruction in Declaration.CustomsEntryInstructions)
			{
				var suspended = entryinstruction.EntryHeader?.IsAllEntryLinesVatSuspended ?? false;
				if (!suspended)
				{
					FiscalRepresentativeDefaulter.DefaultFiscalReferences(entryinstruction);
				}
				else
				{
					var fiscalReferences = entryinstruction.FiscalReferences.Cast<CusFiscalReference>().Where(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative);

					foreach (var fiscalReference in fiscalReferences.ToList())
					{
						entryinstruction.FiscalReferences.RemoveAndDelete(fiscalReference);
					}
				}
			}
		}
	}
}
