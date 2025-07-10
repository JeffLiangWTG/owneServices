using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CNDataObjectReaderHelper : UniversalDataObjectReaderHelper
	{
		public CNDataObjectReaderHelper(UniversalObjectFactory factory, ZString sourceCountryCode, string dataProviderForCodeMapping = null) : base(factory, Core.Constants.CountryCodes.China, sourceCountryCode, dataProviderForCodeMapping)
		{
		}

		public CusEntryNumber LoadOrCreateEntryNumForEntryInstruction(ZGuid parentPK, ZString entryType, bool isParentInDatabase)
		{
			return LoadEntryNumberForEntryInstruction(parentPK, entryType, isParentInDatabase) ?? CreateEntryNumberForEntryInstruction(parentPK, entryType);
		}

		CusEntryNumber LoadEntryNumberForEntryInstruction(ZGuid parentPK, ZString entryType, bool isParentInDatabase)
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, parentPK);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryInstructionSchema.Constants.TableName);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.China);
			query.FetchOnlyFromLocalCache = !isParentInDatabase;

			return Factory.LoadTop1<CusEntryNumber>(query);
		}

		CusEntryNumber CreateEntryNumberForEntryInstruction(ZGuid parentPK, ZString entryType)
		{
			var result = Factory.New<CusEntryNumber>();
			using (result.SuspendSettingHasChanges())
			{
				result.CE_ParentTable = CusEntryInstructionSchema.Constants.TableName;
				result.CE_ParentID = parentPK;
				result.CE_EntryType = entryType;
				result.CE_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			}
			return result;
		}
	}
}
