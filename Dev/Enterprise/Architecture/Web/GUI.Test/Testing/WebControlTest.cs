using System;
using System.Reflection;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	/// <summary>
	/// Base class web control testing
	/// </summary>	
	public abstract class WebControlTest : TestCaseWithFactory
	{
		protected override void RunTest()
		{
			Page.SetServerMappedPathForTest(TestRuntimeDirectory.DirectoryName);
			//Page.DesignerInitialize();
			base.RunTest();
		}

		protected DummyEnterpriseBusinessObject TestBizO
		{
			get
			{
				if (fTestBizO == null)
				{
					fTestBizO = GetNewDataSource();
				}
				return fTestBizO;
			}
		}
		DummyEnterpriseBusinessObject fTestBizO;

		protected TempDirectory TestRuntimeDirectory
		{
			get
			{
				if (fTestRuntimeDirectory == null)
				{
					fTestRuntimeDirectory = new TempDirectory();
				}
				return fTestRuntimeDirectory;
			}
		}
		TempDirectory fTestRuntimeDirectory;

		protected virtual DummyEnterpriseBusinessObject GetNewDataSource()
		{
			DummyEnterpriseBusinessObject result = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			result.Z0_VarCharMax = "ABC";
			return result;
		}

		protected ZTestPage Page
		{
			get
			{
				if (fPage == null)
				{
					fPage = GetNewZTestPage();
					fPage.EnableEventValidation = false;
					fPage.TestDataSource = TestBizO;
				}
				return fPage;
			}
		}
		ZTestPage fPage;

		protected virtual ZTestPage GetNewZTestPage()
		{
			return new ZTestPage();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Control = GetNewControl();
			if (!(Control is WebControls.ZPage))
			{
				Page.Controls.Add(Control);
			}
		}
		protected override void TearDown()
		{
			Control.Dispose();
			TestRuntimeDirectory.Dispose();
			base.TearDown();
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected new BusinessObjectFactory Factory
		{
			get { return base.Factory; }
		}

		protected Control Control;
		protected abstract Control GetNewControl();

		protected string RuntimeVersion
		{
			get
			{
				var assemblyVersionAttribute = (AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(typeof(ZTestPage).Assembly, typeof(AssemblyFileVersionAttribute));

				return (assemblyVersionAttribute != null) ? assemblyVersionAttribute.Version.Replace(".", "_") : "";
			}
		}
	}
}
