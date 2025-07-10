using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyLookups : ZLookups
	{
		public DummyLookups(BusinessObject parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList DummyCodeList
		{
			get
			{
				if (dummyCodeList == null)
				{
					dummyCodeList = new CodeDescriptionPairList();
					dummyCodeList.AddPair("ONE", "Dummy 1");
					dummyCodeList.AddPair("TWO", "Dummy 2");
				}

				return dummyCodeList;
			}
		}

		public DummyBusinessObjectCollection DummyList
		{
			get
			{
				if (dummyList == null)
				{
					dummyList = new DummyBusinessObjectCollection(Factory);
				}

				return dummyList;
			}
		}

		public DummyBusinessObjectCollection DummyList2
		{
			get
			{
				if (dummyList2 == null)
				{
					dummyList2 = new DummyIOrgHeaderBusinessObjectCollection(Factory);
				}

				return dummyList2;
			}
		}

		public DummyWithZAddressCollection DummyWithZAddressList
		{
			get
			{
				if (dummyWithZAddressList == null)
				{
					dummyWithZAddressList = new DummyWithZAddressCollection(Factory);
				}

				return dummyWithZAddressList;
			}
		}

		#region Implementation

		CodeDescriptionPairList dummyCodeList;
		DummyBusinessObjectCollection dummyList;
		DummyBusinessObjectCollection dummyList2;
		DummyWithZAddressCollection dummyWithZAddressList;

		#endregion
	}
}
