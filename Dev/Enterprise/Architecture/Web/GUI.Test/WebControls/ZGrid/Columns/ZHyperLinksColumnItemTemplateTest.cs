using System;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZHyperLinksColumnItemTemplateTest : ZItemTemplateTest
	{
		#region TestCreationAndBinding

		public void TestCreationAndBinding()
		{
			PKDescriptionCollection collection = new PKDescriptionCollection();
			collection.Add(new PKDescription(ZGuid.NewZGuid(), "Description1"));
			collection.Add(new PKDescription(ZGuid.NewZGuid(), "Description2") { DoNotCreateHyperLink = true });

			DummyBusinessObject bizO = new BusinessObjectFactory().New<DummyBusinessObject>();
			bizO.CollectionWithPublicSetter = collection;

			ZHyperLinksColumn column = new ZHyperLinksColumn("Header", "CollectionWithPublicSetter", "Description");
			column.DataNavigateUrlFormatString = dataNavigateUrlFormatString;
			column.DataNavigateUrlFields = new string[] { "PK" };

			ZHyperLinksColumnItemTemplate template = new ZHyperLinksColumnItemTemplate(column);

			ZRepeater repeater = template.GetControl() as ZRepeater;
			Assert("Control has expected type", repeater is ZRepeater);

			((ISelfBindingWebControl)repeater).Bind(bizO);

			Assert("All objects from the collection are bound", repeater.Items.Count == collection.Count);

			AssertBoundedItem(repeater.Items[0].Controls[0], collection[0]);
			AssertSeparator(repeater.Items[1].Controls[0], ", <br>");
			AssertBoundedItem(repeater.Items[1].Controls[2].Controls[0], collection[1]);

			collection.Add(new PKDescription(ZGuid.NewZGuid(), "Description3"));
			collection.Add(new PKDescription(ZGuid.NewZGuid(), "Description4"));
			collection.Add(new PKDescription(ZGuid.NewZGuid(), "Description5"));

			column.MaxItemsCount = 2;
			((ISelfBindingWebControl)repeater).Bind(bizO);

			bool noMoreLinks = true;
			for (int i = 3; i < repeater.Items.Count; i++)
			{
				if (repeater.Items[i].Controls.Count > 0)
				{
					noMoreLinks = false;
					break;
				}
			}

			bool elipsisExists = ((LiteralControl)repeater.Items[2].Controls[0]).Text == "...";

			Assert("Only 2 items and ellipsis should be shown", noMoreLinks && elipsisExists);
		}

		void AssertSeparator(Control control, string expectedText)
		{
			LiteralControl label = control as LiteralControl;
			AssertNotNull("Separator exists", label);
			AssertEquals("Separator text", expectedText, label.Text);
		}

		void AssertBoundedItem(Control control, PKDescription bizO)
		{
			if (bizO.DoNotCreateHyperLink)
			{
				LiteralControl label = control as LiteralControl;
				AssertNotNull("Label exists", label);
				AssertEquals("Label text", bizO.Description, label.Text);
			}
			else
			{
				ZHyperlink link = control as ZHyperlink;
				AssertNotNull("Link exists", link);
				AssertEquals("Link text", bizO.Description, link.Text);
				AssertEquals("Link url", string.Format(dataNavigateUrlFormatString, bizO.PK), link.NavigateUrl);
			}
		}

		readonly string dataNavigateUrlFormatString = "http://somewhere.com?PK={0}";

		#endregion

		#region Overriden Methods

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZHyperLinksColumn); }
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZHyperLinksColumnItemTemplate); }
		}

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			return (ZTemplateColumn)Activator.CreateInstance(ExpectedColumnType, new object[] { header, bindTo, bindTo });
		}

		#endregion
	}
}
