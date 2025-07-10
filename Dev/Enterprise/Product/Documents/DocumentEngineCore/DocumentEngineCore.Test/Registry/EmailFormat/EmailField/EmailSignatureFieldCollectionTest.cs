using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(EmailSignatureFieldCollection))]
	public class EmailSignatureFieldCollectionTest : EmailFieldCollectionTest<EmailSignatureFieldCollection>
	{
		#region Implementation

		protected override EmailSignatureFieldCollection GetCollectionToTest()
		{
			return new EmailSignatureFieldCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EmailSignatureField();
		}

		#endregion
	}
}
