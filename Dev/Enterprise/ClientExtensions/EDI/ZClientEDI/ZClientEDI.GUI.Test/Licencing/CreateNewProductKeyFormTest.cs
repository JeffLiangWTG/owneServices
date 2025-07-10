using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(CreateNewProductKeyForm))]
	public class CreateNewProductKeyFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CreateNewProductKeyForm();
		}

		public void TestInvalidServerCodeEntered()
		{
			using (var form = new CreateNewProductKeyFormForTesting())
			{
				form.PreferredServerCodeTextBoxExposed.Text = @"111";
				form.OkButton_Click(this, EventArgs.Empty);

				AssertEquals(form.InvalidServerCodeMessageExposed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNumberOfProductKeysForUserInEnterpriseHasReachedLimit()
		{
			using (var form = new CreateNewProductKeyFormForTesting())
			{
				form.CreateProductKeys(form.AdditionalServerCodes);

				form.PreferredServerCodeTextBoxExposed.Text = @"222";
				form.OkButton_Click(this, EventArgs.Empty);

				AssertEquals(form.NumberOfProductKeysReachedLimitMessageExposed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestProductKeyRegistered()
		{
			using (var form = new CreateNewProductKeyFormForTesting())
			{
				form.PreferredServerCodeTextBoxExposed.Text = @"A06";
				form.OkButton_Click(this, EventArgs.Empty);

				AssertEquals(form.ProductKeySuccessfullyCreatedMessageExposed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestServerCodeAlreadyInUse()
		{
			using (var form = new CreateNewProductKeyFormForTesting())
			{
				form.PreferredServerCodeTextBoxExposed.Text = @"A01";
				form.OkButton_Click(this, EventArgs.Empty);

				AssertEquals(form.ServerCodeAlreadyInUseMessageExposed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public class CreateNewProductKeyFormForTesting : CreateNewProductKeyForm
		{
			#region Private Variables

			BusinessObjectFactory factory;
			LicenceEnterprise licenceEnterprise;
			EDIOrgHeader orgHeader;

			#endregion

			#region Properties

			internal MultilingualString InvalidServerCodeMessageExposed => InvalidServerCodeMessage;

			internal MultilingualString NumberOfProductKeysReachedLimitMessageExposed => NumberOfProductKeysReachedLimitMessage;

			internal MultilingualString ProductKeySuccessfullyCreatedMessageExposed => ProductKeySuccessfullyCreatedMessage;

			internal MultilingualString ServerCodeAlreadyInUseMessageExposed => ServerCodeAlreadyInUseMessage;

			internal ZArchitecture.ZTextBox PreferredServerCodeTextBoxExposed => PreferredServerCodeTextBox;

			internal List<string> ServerCodes => new List<string>() { "A01", "A02", "A03", "A04", "A05" };

			internal List<string> AdditionalServerCodes => new List<string>() { "A06", "A07", "A08", "A09", "A10" };

			internal LicenceEnterprise LicenceEnterprise
			{
				get
				{
					if (licenceEnterprise == null)
					{
						licenceEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
						licenceEnterprise.LE_EnterpriseCode = LicenceEnterpriseWTL;
						licenceEnterprise.LE_OH = OrgHeader.PK;
					}
					return licenceEnterprise;
				}
			}

			internal EDIOrgHeader OrgHeader
			{
				get
				{
					if (orgHeader == null)
					{
						orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
						orgHeader.OH_FullName = "Test Company";
						orgHeader.OH_Code = "EDITEST";
					}
					return orgHeader;
				}
			}

			internal BusinessObjectFactory Factory
			{
				get
				{
					if (factory == null)
					{
						factory = new BusinessObjectFactory();
					}
					return factory;
				}
			}

			#endregion

			#region Constructor

			public CreateNewProductKeyFormForTesting()
			{
				PrepareTestData();
			}

			#endregion

			#region Methods

			internal void CreateProductKeys(List<string> serverCodes)
			{
				foreach (var serverCode in serverCodes)
				{
					var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
					licenceDatabase.LD_LE = LicenceEnterprise.PK;
					licenceDatabase.LD_ServerCode = serverCode;
					licenceDatabase.LD_GS_NKOwner = GlbStaff.CurrentUser.GS_Code;
				}

				Factory.Save();
			}

			internal void PrepareTestData()
			{
				CreateProductKeys(ServerCodes);
			}

			#endregion
		}
	}
}
