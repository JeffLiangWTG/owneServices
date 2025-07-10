using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class CustomsEntryLineDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryLineDataObjectWriter
	{
		public CustomsEntryLineDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected new UniversalDataObjectWriterHelper helper
		{
			get { return (UniversalDataObjectWriterHelper)base.helper; }
		}

		protected override void PopulateCusEntrySupportingInfoData(CusEntryLine entryLineBO, EntryLine entryLineData)
		{
			base.PopulateCusEntrySupportingInfoData(entryLineBO, entryLineData);
			entryLineData.CustomsSupportingInformationCollection = CreateCollection((Business.Declaration.CusEntryLine)entryLineBO, writeManager);
		}

		protected List<CustomsSupportingInformation> CreateCollection(Business.Declaration.CusEntryLine entryLine, IDataWritingManager writeManager)
		{
			var result = new List<CustomsSupportingInformation>();
			if (entryLine != null)
			{
				var typeList = CusSupportingInfoTypeListProvider.GetListForList();
				entryLine.SupportingDocuments.ToList().ForEach(x => result.Add(Create(x, typeList, writeManager)));
				entryLine.PreviousDocuments.ToList().ForEach(x => result.Add(Create(x, typeList, writeManager)));
				entryLine.AdditionalInfos.ToList().ForEach(x => result.Add(Create(x, typeList, writeManager)));
			}
			return result.Count == 0 ? null : result;
		}

		CustomsSupportingInformation Create(CusSupportingInfo cusSupportingInfo, ICodeDescriptionPairList typeList, IDataWritingManager writeManager)
		{
			var supportingInfo = new CustomsSupportingInformation()
			{
				Category = ListHelper.GetWithDescription<CodeDescriptionPair>(cusSupportingInfo.CSI_Type, typeList),
				Country = Country.NewOrEmpty(cusSupportingInfo.Country),
				CustomsOffice = ListHelper.GetWithDescription<CodeDescriptionPair10Char>(cusSupportingInfo.CSI_CustomsOffice, cusSupportingInfo.Lookups.CustomsOfficeList),
				DateOfIssue = cusSupportingInfo.CSI_DateOfIssue.Date,
				Description = cusSupportingInfo.CSI_Description,
				LineNo = cusSupportingInfo.CSI_LineNo,
				Procedure = ListHelper.GetWithDescription<CodeDescriptionPair7Char>(cusSupportingInfo.CSI_Procedure, cusSupportingInfo.Lookups.ProcedureList),
				Quantity = cusSupportingInfo.CSI_Quantity,
				Quantity2 = cusSupportingInfo.CSI_Quantity2,
				ReferenceNumber = cusSupportingInfo.CSI_ReferenceNumber,
				Status = ListHelper.GetWithDescription<CodeDescriptionPair>(cusSupportingInfo.CSI_Status, cusSupportingInfo.Lookups.StatusList),
				SubType = ListHelper.GetWithDescription<CodeDescriptionPair5Char>(cusSupportingInfo.CSI_SubType, cusSupportingInfo.Lookups.SubTypeList),
				UnitOfQuantity = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(cusSupportingInfo.CSI_UnitOfQuantity, cusSupportingInfo.Lookups.UnitOfQuantityList),
				UnitOfQuantity2 = ListHelper.GetWithDescription<CodeDescriptionPair4Char>(cusSupportingInfo.CSI_UnitOfQuantity2, cusSupportingInfo.Lookups.UnitOfQuantity2List)
			};

			if (cusSupportingInfo.Lookups.CodeList is ICodeDescriptionPairList codeList)
			{
				supportingInfo.Type = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(cusSupportingInfo.CSI_Code, codeList);
			}
			else if (cusSupportingInfo.Lookups.CodeList is IFindBoxListProvider listProvider)
			{
				supportingInfo.Type = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(cusSupportingInfo.CSI_Code, listProvider);
			}

			writeManager.NotifyExported(supportingInfo, cusSupportingInfo);
			return supportingInfo;
		}
	}
}
