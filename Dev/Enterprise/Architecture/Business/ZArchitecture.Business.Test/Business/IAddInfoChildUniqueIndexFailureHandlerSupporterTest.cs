using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestsSubclassesOf(typeof(IAddInfoChildUniqueIndexFailureHandlerSupporter))]
	public abstract class IAddInfoChildUniqueIndexFailureHandlerSupporterTestCase<T> : EnterpriseBusinessObjectTestCase
		where T : EnterpriseBusinessObject, IAddInfoChildUniqueIndexFailureHandlerSupporter
	{
		public void TestIAddInfoChildUniqueIndexFailureHandlerSupporterMembers()
		{
			var bizObj = (T)GetNewBusinessObject();
			var parent = GetParent(bizObj);
			SetUpSystemLastEdit(bizObj, parent, "!23", ZDateTime.BrettsBirthday);
			IAddInfoChildUniqueIndexFailureHandlerSupporter supporter = bizObj;
			AssertEquals("UniqueIndexName", ExpectedUniqueIndexName, supporter.UniqueIndexName);
			AssertSame("Parent", parent, supporter.Parent);
			AssertEquals("SystemLastEditUser", "!23", supporter.SystemLastEditUser);
			AssertEquals("SystemLastEditTimeUtc", ZDateTime.BrettsBirthday, supporter.SystemLastEditTimeUtc);
		}

		void SetUpSystemLastEdit(T bizObj, EnterpriseBusinessObject parent, ZString systemLastEditUser, ZDateTime systemLastEditTimeUtc)
		{
			var tablePrefix = bizObj.TablePrefix;
			var tableSchema = bizObj.PKSchemaColumn.TableSchema;
			var systemLastEditUserColumnName = tablePrefix + "_SystemLastEditUser";
			if (tableSchema.GetSchemaColumn(systemLastEditUserColumnName) == null)
			{
				SetUpSystemLastEditOnParent(parent, systemLastEditUser, systemLastEditTimeUtc);
			}
			else
			{
				bizObj[systemLastEditUserColumnName] = systemLastEditUser;
				bizObj[tablePrefix + "_SystemLastEditTimeUtc"] = systemLastEditTimeUtc;
			}
		}

		protected virtual void SetUpSystemLastEditOnParent(EnterpriseBusinessObject parent, ZString systemLastEditUser, ZDateTime systemLastEditTimeUtc)
		{
		}

		protected abstract EnterpriseBusinessObject GetParent(T bizObj);

		protected abstract string ExpectedUniqueIndexName { get; }
	}
}
