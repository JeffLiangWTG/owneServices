using Enterprise.Client.OIA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.OIA.GUI.Testing
{
	[TestedType(typeof(OIAGLTransactionsForm))]
	public class OIAGLTransactionsFormTest : ZFormBasherTest
	{
		public void TestFormVerbAndCaption()
		{
			using (OIAGLTransactionsFormForTest form = new OIAGLTransactionsFormForTest(new OIAGLTransactionBusinessObject(Factory)))
			{
				AssertEquals("Form verb", "", form.FormVerb);
			}
		}

		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new OIAGLTransactionsForm(new OIAGLTransactionBusinessObject(Factory));
		}

		class OIAGLTransactionsFormForTest : OIAGLTransactionsForm
		{
			public OIAGLTransactionsFormForTest(OIAGLTransactionBusinessObject bizo) : base(bizo)
			{
			}

			public ZGroupBox DirectoryGroupBoxExposed
			{
				get
				{
					return base.DirectoryGroupBox;
				}
			}

			public ZTextBox ExportDirectoryTextBoxExposed
			{
				get
				{
					return base.ExportDirectoryTextBox;
				}
			}

			public ZButton BrowseButtonExposed
			{
				get
				{
					return base.BrowseButton;
				}
			}

			public ZPeriodEdit FromPeriodPeriodEditExposed
			{
				get
				{
					return base.FromPeriodPeriodEdit;
				}
			}

			public ZPeriodEdit ToPeriodPeriodEditExposed
			{
				get
				{
					return base.ToPeriodPeriodEdit;
				}
			}

			public ZGuidFindBox StartGLAccountGuidFindBoxExposed
			{
				get
				{
					return base.StartGLAccountGuidFindBox;
				}
			}

			public ZGuidFindBox EndGLAccountGuidFindBoxExposed
			{
				get
				{
					return base.EndGLAccountGuidFindBox;
				}
			}

			public ZGuidFindBox DepartmentPKGuidFindBoxExposed
			{
				get
				{
					return base.DepartmentPKGuidFindBox;
				}
			}

			public ZGuidFindBox BranchPKGuidFindBoxExposed
			{
				get
				{
					return base.BranchPKGuidFindBox;
				}
			}

			public ZDropEdit DescriptionDisplayDropEditExposed
			{
				get
				{
					return base.DescriptionDisplayDropEdit;
				}
			}

			public ZGroupBox SortByGroupBoxExposed
			{
				get
				{
					return base.SortByGroupBox;
				}
			}
		}
	}
}
