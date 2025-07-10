using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICusHAWB
			{
				ZGuid PK { get; }

				#region CS_ApplicationCode

				ZString CS_ApplicationCode
				{
					get;
					set;
				}

				ZPropertyInfo CS_ApplicationCodeInfo
				{
					get;
				}

				#endregion

				#region CS_CM

				ZGuid CS_CM
				{
					get;
					set;
				}

				ZPropertyInfo CS_CMInfo
				{
					get;
				}

				#endregion

				#region CS_HAWB

				ZString CS_HAWB
				{
					get;
					set;
				}

				ZPropertyInfo CS_HAWBInfo
				{
					get;
				}

				#endregion

				#region CS_JS

				ZGuid CS_JS
				{
					get;
					set;
				}

				ZPropertyInfo CS_JSInfo
				{
					get;
				}

				#endregion

				#region CS_MasterHouseBill

				ZString CS_MasterHouseBill
				{
					get;
					set;
				}

				ZPropertyInfo CS_MasterHouseBillInfo
				{
					get;
				}

				#endregion

				#region CS_SystemCreateTimeUtc

				ZDateTime CS_SystemCreateTimeUtc
				{
					get;
				}

				#endregion

				bool IsInDatabase { get; }

				ICusMAWB MAWB
				{
					get;
				}
			}
		}
	}
}