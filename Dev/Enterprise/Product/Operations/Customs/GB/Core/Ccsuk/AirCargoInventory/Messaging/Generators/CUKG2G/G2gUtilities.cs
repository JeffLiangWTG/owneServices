using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	static class G2gUtilities
	{
		public enum CodeType
		{
			/// <summary>
			/// Customs Authorisation Reference
			/// </summary>
			CAR,

			/// <summary>
			/// External UCR
			/// </summary>
			UCR
		}

		internal static IEnumerable<CusEntryNumber> GetNumbers(CodeType typeCode, CusEntryNumAdditionalReferenceCollection cusEntryNumAdditionalReferenceCollection)
		{
			return (from CusEntryNumber n in cusEntryNumAdditionalReferenceCollection where n.CE_EntryType == typeCode.ToString() select n);
		}

		internal static ZString GetFirstNumber(CodeType typeCode, CusEntryNumAdditionalReferenceCollection cusEntryNumAdditionalReferenceCollection)
		{
			var number = GetNumbers(typeCode, cusEntryNumAdditionalReferenceCollection).FirstOrDefault();
			return number != null ? number.CE_EntryNum : ZString.Empty;
		}

		internal static ZString GetAgentFromPima(ZString pima)
		{
			return pima.Right(3);
		}

		internal static ZString GetAgentTypeFromPima(ZString pima)
		{
			var result = ZString.Empty;
			foreach (CredentialsSetting credential in GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				if (credential.PIMA == pima)
				{
					result = credential.CcsukFallbackAgentType;
					break;
				}
			}
			return result;
		}
	}
}
