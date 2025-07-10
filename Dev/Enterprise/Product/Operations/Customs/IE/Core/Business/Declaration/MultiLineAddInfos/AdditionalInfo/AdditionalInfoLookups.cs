using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class AdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
	{
		public AdditionalInfoLookups(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo parent) : base(parent)
		{
		}

		protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

		protected override ZBool OmitLevelAttribute => true;

		protected override ZBool UseUCCAdditionalInfosCore(BusinessObject parent)
		{
			if (parent is CusEntryInstruction instruction && instruction.JobDeclaration is JobDeclaration declaration)
			{
				return declaration.Configuration.UCCAdditionalInfosSupport(declaration);
			}
			else
			{
				return base.UseUCCAdditionalInfosCore(parent);
			}
		}

		protected override ZString[] GetRefCusCodeListTypes(EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport importExportParent, ZString csiSubType)
		{
			var codeType = ZString.Empty;
			if (importExportParent.IsImport)
			{
				switch (csiSubType)
				{
					case AdditionalInfoSubTypeList.Codes.AdditionalReference:
						codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportAddDocAdditionalReference;
						break;
				}
			}
			else if (importExportParent.IsExport)
			{
				switch (csiSubType)
				{
					case AdditionalInfoSubTypeList.Codes.AdditionalInformation:
						codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportAddDocAdditionalInformation;
						break;
					case AdditionalInfoSubTypeList.Codes.AdditionalReference:
						codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportAddDocAdditionalReference;
						break;
				}
			}
			return codeType.IsEmpty ? base.GetRefCusCodeListTypes(importExportParent, csiSubType) : new ZString[] { codeType };
		}

		public override CodeDescriptionPairList SubTypeList
		{
			get
			{
				var parent = Parent;
				var removeAdditionalReference = ShouldRemoveAdditionalReference(parent);
				var removeTransportDocument = ShouldRemoveTransportDocument(parent);
				return Factory.GetCachedValue($"IE.AdditionalInfoLookups.SubTypeList_{removeAdditionalReference}_{removeTransportDocument}", () =>
				{
					var result = new AdditionalInfoSubTypeList();
					if (removeAdditionalReference)
					{
						result.RemoveCode(EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference);
					}
					if (removeTransportDocument)
					{
						result.RemoveCode(EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument);
					}
					return result;
				});

				bool ShouldRemoveAdditionalReference(AdditionalInfo addInfo) => ShouldRemoveForUCC5(addInfo);

				bool ShouldRemoveTransportDocument(AdditionalInfo addInfo) => (!addInfo.ParentIsInvoiceHeader && !addInfo.ParentIsCusEntryInstruction && !addInfo.ParentIsJobComInvoiceLine && !addInfo.ParentIsCusClassPartPivot) || ShouldRemoveForUCC5(addInfo);

				bool ShouldRemoveForUCC5(AdditionalInfo addInfo) => addInfo.Declaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport;
			}
		}
	}
}
