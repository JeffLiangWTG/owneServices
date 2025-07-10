using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(EmailSignatureField))]
	public class EmailSignatureFieldTest : EmailFieldTest
	{
		public void TestParentCollection()
		{
			AssertEquals(typeof(EmailSignatureFieldCollection), BizObj.ParentCollectionInternal.GetType());
		}

		public void TestEmailFieldsPairList()
		{
			EmailSignatureField bizObj = new EmailSignatureField();
			AssertEquals(bizObj.EmailFieldsPairList.Count, DocumentsDataRegistry.Instance.EmailSignatureFieldsPairList.Count);
			foreach (CodeDescriptionPair pair in DocumentsDataRegistry.Instance.EmailSignatureFieldsPairList)
			{
				bizObj.EmailFieldsPairList.Contains(pair);
			}
		}

		#region Overrides

		protected override void SetParentCollection()
		{
			EmailSignatureFieldCollection collection = new EmailSignatureFieldCollection();
			collection.Add(BizObj);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new EmailSignatureField();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new EmailSignatureField();
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

		protected new EmailSignatureField BizObj
		{
			get
			{
				return (EmailSignatureField)base.BizObj;
			}
		}

		#endregion
	}
}
