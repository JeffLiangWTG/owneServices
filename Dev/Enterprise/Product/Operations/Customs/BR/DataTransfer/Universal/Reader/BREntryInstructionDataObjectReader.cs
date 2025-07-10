using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class BREntryInstructionDataObjectReader : CustomsEntryInstructionDataObjectReader
	{
		public BREntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, UniversalObjectFactory factory, BaseJobDeclaration declaration) : base(entryInstructionDataObject, logger, helper, factory, declaration)
		{
		}

		protected override void FillCountrySpecificDetails(CusEntryInstruction targetBO)
		{
			base.FillCountrySpecificDetails(targetBO);

			var brHelper = helper as BRDataObjectReaderHelper;
			if (brHelper.IsSourceAndTargetCountrySame && dataObject.AddInfoCollection != null)
			{
				var entryInstructionPK = targetBO.PK;

				var ucrNumberAddInfo = dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.EntryInstruction.UCRNumber, logger);
				var isUCROverriddenAddInfo = dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.EntryInstruction.IsUCROverridden, logger);
				if (ucrNumberAddInfo.HasValue || isUCROverriddenAddInfo.HasValue)
				{
					var entryNumRow = GetColumnIndexer(brHelper.LoadOrCreateEntryNumForEntryInstruction(entryInstructionPK, CusEntryNumberTypes.Standard.UniqueConsignementReference, targetBO.IsInDatabase));

					if (ucrNumberAddInfo.HasValue)
					{
						SetValue(entryNumRow, CusEntryNumSchema.CE_EntryNum, ucrNumberAddInfo);
					}
					if (isUCROverriddenAddInfo.HasValue)
					{
						SetValue(entryNumRow, CusEntryNumSchema.CE_EntryIsSystemGenerated, isUCROverriddenAddInfo.Value != "Y");
					}
				}
			}
		}
	}
}
