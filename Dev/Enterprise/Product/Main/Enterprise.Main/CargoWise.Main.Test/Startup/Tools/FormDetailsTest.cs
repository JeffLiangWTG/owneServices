using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(FormDetails))]
	sealed class FormDetailsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new FormDetails(new Form());
		}
	}
}
