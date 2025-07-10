using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class OperationMatterCollection : CusCodeDataCollection<OperationMatter>, ICodeDescriptionOptionStorage
	{
		public OperationMatterCollection(CusEntryInstruction parent) : base(parent, Constants.CusCodeDataTypes.Codes.OperationMatter)
		{
		}

		public new CusEntryInstruction Master => base.Master as CusEntryInstruction;

		public IValidationModeProvider ValidationModeProvider => Master.JobDeclaration;

		#region ICodeDescriptionOptionStorage
		public IEnumerable<ZString> AllCodes => this.Cast<OperationMatter>().Select(codeData => codeData.CY_Code);

		public ZPropertyInfo SelectedOptionsAsStringPropertyInfo => Master.OperationMattersAsStringInfo;

		public BusinessObject FindByCode(ZString code)
		{
			return GetFirstElementHaving(code);
		}

		public ICodeDescriptionPairList GetAllOptions()
		{
			return Factory.GetCachedValue<ICodeDescriptionPairList>("Customs.CN.Business.OperationMatterListRemovePTFAndATF", () =>
			{
				var codeDescriptionPairList = new OperationMatterList();
				codeDescriptionPairList.RemoveCode(OperationMatterList.Codes.PaperlessTaxForm);
				codeDescriptionPairList.RemoveCode(OperationMatterList.Codes.AutonomousTaxFiling);
				return codeDescriptionPairList;
			});
		}

		public void ValidateSeletedOption(ZString code, bool selected, ZPropertyInfo propertyInfo, IEnumerable<ZString> selectedCodes)
		{
			if (selected)
			{
				var description = Master.OperationMatters.GetAllOptions().GetDescriptionFromCode(code);
				if (code == OperationMatterList.Codes.AssuredInspectClearance && Master.CEI_DocumentSubmissionType != EntryDocumentSubmissionTypes.Codes.PaperlessForCustoms)
				{
					propertyInfo.AddNotification(Res.GetString("a4d5f7e9-110c-435d-99d4-efa86ded047b", "Only '{0}' entry supports {1}.", EntryDocumentSubmissionTypes.Descriptions.PaperlessForCustoms, description), ValidationModeProvider);
				}

				if (code == OperationMatterList.Codes.ConsolidatedDutyCollection)
				{
					if (Master.WillGenerateRecordListing)
					{
						propertyInfo.AddNotification(Res.GetString("5049276e-6b82-4d60-b957-f29b3853e079", "'{0}' does not support {1}.", OperationMatterList.Descriptions.ConsolidatedDutyCollection, DecTypeList.Descriptions.RecordListing), ValidationModeProvider);
					}

					ZString[] dutyModes = { DutyModeList.Codes._3, DutyModeList.Codes._6, DutyModeList.Codes._7 };

					if (Master.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => dutyModes.Contains(x.JI_DutyMode)))
					{
						propertyInfo.AddNotification(Res.GetString("31F719C1-239B-4EB6-A283-087AFCAD98A0", "'{0}' should not be selected due to some Invoice Lines with Duty Mode {1}.", OperationMatterList.Descriptions.ConsolidatedDutyCollection, ZString.Join(",", dutyModes)), ValidationModeProvider);
					}
				}
			}
		}

		void ICodeDescriptionOptionStorage.AddNew(ZString code)
		{
			AddNew(code);
		}

		#endregion
	}
}
