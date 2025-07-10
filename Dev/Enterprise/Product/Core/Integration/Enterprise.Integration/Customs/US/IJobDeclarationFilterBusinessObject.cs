using System.Collections;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public interface IJobDeclarationFilterBusinessObject
			{
				ZString ReleaseStatusDescription { get; }
				[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
				IList ReleaseStatusList { get; }
				object GetReleaseStatusTextQueryWithOperator { get; }

				ZString FDAMsgStatusDescription { get; }
				[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
				IList FDAMsgStatusList { get; }
				object GetFDAMsgStatusTextQueryWithOperator { get; }

				ZString FDAStatusDescription { get; }
				[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
				IList FDAStatusList { get; }
				object GetFDAStatusTextQueryWithOperator { get; }
			}
		}
	}
}
