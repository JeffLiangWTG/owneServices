using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(EmailSubjectField))]
	public class EmailSubjectFieldTest : EmailFieldTest
	{
		public void TestParentCollection()
		{
			AssertEquals(typeof(EmailSubjectFieldCollection), BizObj.ParentCollectionInternal.GetType());
		}

		public void TestEmailFieldsPairList()
		{
			EmailSubjectField bizObj = new EmailSubjectField();
			AssertEquals(bizObj.EmailFieldsPairList.Count, DocumentsDataRegistry.Instance.EmailSubjectFieldsPairList.Count);
			foreach (CodeDescriptionPair pair in DocumentsDataRegistry.Instance.EmailSubjectFieldsPairList)
			{
				bizObj.EmailFieldsPairList.Contains(pair);
			}
		}

		#region Overrides

		protected override void SetParentCollection()
		{
			EmailSubjectFieldCollection collection = new EmailSubjectFieldCollection();
			collection.Add(BizObj);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new EmailSubjectField();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new EmailSubjectField();
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected new EmailSubjectField BizObj
		{
			get
			{
				return (EmailSubjectField)base.BizObj;
			}
		}
		#endregion
	}
}
