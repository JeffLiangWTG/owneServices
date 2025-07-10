using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class BREntryInstructionDataObjectWriter : CustomsEntryInstructionDataObjectWriter
	{
		public BREntryInstructionDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		protected override List<AddInfo> GetEntryInstructionAddInfoCollection(CusEntryInstruction instructionBO)
		{
			var addInfoList = base.GetEntryInstructionAddInfoCollection(instructionBO) ?? new List<AddInfo>();

			var instructionBoBR = instructionBO as Business.CusEntryInstruction;
			if (instructionBoBR != null)
			{
				addInfoList.Add(new AddInfo()
				{
					Key = Constants.AddInfoKeys.EntryInstruction.UCRNumber,
					Value = instructionBoBR.UCRNumber
				});
				addInfoList.Add(new AddInfo()
				{
					Key = Constants.AddInfoKeys.EntryInstruction.IsUCROverridden,
					Value = instructionBoBR.IsUCROverridden ? "Y" : "N"
				});
			}

			return addInfoList;
		}
	}
}
