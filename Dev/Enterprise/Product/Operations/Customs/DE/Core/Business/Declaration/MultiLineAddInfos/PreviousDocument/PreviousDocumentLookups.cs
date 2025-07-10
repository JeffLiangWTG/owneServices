using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class PreviousDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentLookups
	{
		public PreviousDocumentLookups(PreviousDocument parent)
			: base(parent)
		{
		}

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

		public static CodeDescriptionPairList GetProcedureList(bool isImport)
		{
			CodeDescriptionPairList result;
			if (isImport)
			{
				result = new PreviousProcedureList();
			}
			else
			{
				result = new CodeDescriptionPairList();
				result.AddPair(PreviousProcedureList.Codes._ATAV, PreviousProcedureList.Descriptions._ATAV);
				result.AddPair(PreviousProcedureList.Codes._ATZL, PreviousProcedureList.Descriptions._ATZL);
				result.Sort();
			}
			return result;
		}

		public ZZRefCusCodeListCombinedCollection DocumentCodeList
		{
			get
			{
				var levelAttributeValue = Parent.IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments ? EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item : EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header;
				var excludedCodesList = new[] { UniversalReferenceConstants.SupportingDocumentTypes._9ZZX, UniversalReferenceConstants.SupportingDocumentTypes._9ZZY };
				var excludeCodesQuery = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.NotEqual, excludedCodesList);
				return CusSupportingInfoHelper.GetDocumentCodesFilteredByLevelAttributes(Factory, excludeCodesQuery, new ZString[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E }, levelAttributeValue);
			}
		}

		public CodeDescriptionPairList InvoiceLineNumberList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (Parent.Parent is CusEntryInstruction cusEntryInstruction)
				{
					result.AddRange(cusEntryInstruction.InvoiceLines.Select(x => new CodeDescriptionPair(x.JI_LineNo.ToString(), Res.GetString("049C0B54-7497-4CAF-8C05-ED4BF623B76B", "Inv. No. {0}", x.InvoiceHeader.JZ_InvoiceNumber))).ToArray());
					result.Sort();
				}
				return result;
			}
		}

		public override CodeDescriptionPairList ProcedureList => Factory.GetCachedValue(string.Join("|", "DE.PreviousDocumentLookups.ProcedureList", Parent.ImportExportParent.IsImport), () => GetProcedureList(Parent.ImportExportParent.IsImport));

		public override CodeDescriptionPairList SubTypeList
		{
			get
			{
				var result = base.SubTypeList;
				if (Parent.IsImport)
				{
					var parent = Parent;
					if (parent.IsProcedureATNEU)
					{
						var invoice = (IPreviousDocumentParentProvider)parent.Parent;
						var declaration = invoice.JobDeclaration;
						var officeCode = declaration?.JE_CustomsOffice ?? ZString.Empty;
						result = Factory.GetCachedSubTypeList_ATNEU(officeCode);
					}
					else
					{
						result = Factory.GetCachedValue<PreviousDocSubTypeList>();
					}
				}

				return result;
			}
		}

		public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<PreviousStatusList>();

		public override CodeDescriptionPairList UnitOfQuantityList
		{
			get
			{
				var parent = Parent;
				CodeDescriptionPairList result;
				if (parent.IsInExportInvoiceLineOrCusClassPartPivotPreviousDocuments && parent.CodeCusCodeList.MissesAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit))
				{
					result = new CodeDescriptionPairList();
				}
				else
				{
					result = CachedCustomsUQList;
				}
				return result;
			}
		}

		public override CodeDescriptionPairList UnitOfQuantity2List => CachedCustomsUQList;

		CodeDescriptionPairList CachedCustomsUQList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today, includeParentDataGrouping: false);
	}
}
