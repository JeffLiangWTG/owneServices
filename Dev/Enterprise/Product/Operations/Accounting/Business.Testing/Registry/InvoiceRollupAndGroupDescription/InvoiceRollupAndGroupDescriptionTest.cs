using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceRollupAndGroupDescription))]
	public class InvoiceRollupAndGroupDescriptionTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestSettingEnglishDescriptionSetsDescription()
		{
			var item = new InvoiceRollupAndGroupDescription();

			item.EnglishDescription = ZString.Empty;
			AssertEquals(item.EnglishDescription, item.Description.GetUnresolvedString());

			item.EnglishDescription = "description";
			AssertEquals(item.EnglishDescription, item.Description.GetUnresolvedString());
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new InvoiceRollupAndGroupDescription(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty), Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			var bizObj = new InvoiceRollupAndGroupDescription();
			bizObj.Style = "ALL";
			bizObj.Group = "ALL";
			bizObj.Description = (NoResString)"All charges except Customs Duty and Tax";

			return bizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceRollupAndGroupDescription();
		}

		protected new InvoiceRollupAndGroupDescription BizObj
		{
			get { return (InvoiceRollupAndGroupDescription)base.BizObj; }
		}

		#endregion
	}
}
