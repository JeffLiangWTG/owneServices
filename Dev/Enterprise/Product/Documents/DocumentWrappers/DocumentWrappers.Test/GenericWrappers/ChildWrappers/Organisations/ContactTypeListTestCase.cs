using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.DocumentWrappers.GenericWrappers.ContactTypeList;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class ContactTypeListTestCase : TestCaseWithFactory
	{
		public void TestAllContactTypesInMasterFilesAreImplementedInThisListToo()
		{
			List<ContactType> exclusions = new List<ContactType>(new ContactType[]
						{
								ContactType.Miscellaneous,
								ContactType.NoContactType
						});

			ContactTypeList contactTypeList = new ContactTypeList();
			FieldInfo[] fieldInfos = typeof(ContactType).GetFields();
			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				if (fieldInfo.IsPublic && fieldInfo.IsStatic && fieldInfo.FieldType == typeof(ContactType))
				{
					ContactType contactType = (ContactType)fieldInfo.GetValue(null);
					if (!exclusions.Contains(contactType))
					{
						bool gotThisOneCovered = false;
						foreach (ContactTypeItem contactTypeItem in contactTypeList)
						{
							if (contactTypeItem.ContactType == contactType)
							{
								gotThisOneCovered = true;
								break;
							}
						}
						Assert(GetHowToFixThisUp(contactType), gotThisOneCovered);
					}
				}
			}
		}

		string GetHowToFixThisUp(ContactType contactType)
		{
			return @"

Another static ContactType field has been created on the class ContactType in MasterFiles.
   (" + contactType.Code + " - " + contactType.DefaultName + @")

Please either implement a matching ContactTypeItem on the ContactTypeList here in 
DocumentWrappers, or add it to the list of exclusions at the beginning of this test.

If you choose to exclude this ContactType, it will not be available for use using
the indexer on a Contact Collection when implementing any documents.

";
		}
	}
}
