using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Customs.CN.Business.Extensions;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CNEntryInstructionDataObjectWriter : CustomsEntryInstructionDataObjectWriter
	{
		public CNEntryInstructionDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		protected override List<AddInfo> GetEntryInstructionAddInfoCollection(CusEntryInstruction instructionBO)
		{
			var addInfoList = base.GetEntryInstructionAddInfoCollection(instructionBO) ?? new List<AddInfo>();

			if (instructionBO is Business.CusEntryInstruction instructionBoCN)
			{
				addInfoList.Add(new AddInfo
				{
					Key = Constants.AddInfoKeys.EntryInstruction.CustomsMessageRemarks,
					Value = instructionBoCN.CustomsMessageRemarks
				});
				addInfoList.Add(new AddInfo
				{
					Key = Constants.AddInfoKeys.EntryInstruction.BillOfLading,
					Value = instructionBoCN.BillOfLading
				});
				addInfoList.Add(new AddInfo
				{
					Key = Constants.AddInfoKeys.EntryInstruction.BillOfLadingDate,
					Value = instructionBoCN.BillOfLadingDate.ToISO8601String()
				});
			}

			return addInfoList;
		}

		protected override List<AddInfoGroup> GetEntryInstructionAddInfoGroupCollection(CusEntryInstruction instructionBO)
		{
			var result = base.GetEntryInstructionAddInfoGroupCollection(instructionBO) ?? new List<AddInfoGroup>();
			if (instructionBO is Business.CusEntryInstruction instructionBoCN)
			{
				PopulateAttachment(result, instructionBoCN, new CodeDescriptionPair
				{
					Code = Constants.EntryInstruction.Codes.EIA,
					Description = Constants.EntryInstruction.Descriptions.EIA
				});
			}
			return result;
		}

		void PopulateAttachment(List<AddInfoGroup> result, Business.CusEntryInstruction instructionBoCN, CodeDescriptionPair codeDescriptionPair)
		{
			foreach (var entryInstructionAttachment in instructionBoCN.Attachments.Cast<Business.EntryInstructionAttachment>())
			{
				var infoGroup = new AddInfoGroup
				{
					Type = codeDescriptionPair,
					AddInfoCollection = new List<AddInfo>()
				};
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.Attachment.Keys.AttachmentFileName, entryInstructionAttachment.FileName);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.Attachment.Keys.AttachmentType, entryInstructionAttachment.AttachmentType);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.Attachment.Keys.AttachmentNumber, entryInstructionAttachment.AttachmentNumber);

				var entryLineNumbers = entryInstructionAttachment.GetLinkedEntryLineNumbers();
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.Attachment.Keys.EntryLineLinks, entryLineNumbers.JoinAsString());
				result.Add(infoGroup);
			}
		}

		void UpdateAddInfoCollection(List<AddInfo> addInfoList, ZString key, IZType value)
		{
			if (!value.IsEmpty && !value.IsDefault)
			{
				helper.Update(addInfoList, key, value);
			}
		}
	}
}
