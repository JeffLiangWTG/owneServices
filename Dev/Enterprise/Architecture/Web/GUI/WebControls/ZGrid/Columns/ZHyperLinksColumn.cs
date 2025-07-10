using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZHyperLinksColumn : ZTemplateColumn
	{
		public ZHyperLinksColumn(string headerText, string bindTo, string bindToField)
			: base(headerText, bindTo)
		{
			this.bindToField = bindToField;
		}

		#region Item Templates

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZHyperLinksColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return GetItemTemplate();
		}

		#endregion

		#region Properties

		public string BindToField
		{
			get { return bindToField; }
			set { bindToField = value; }
		}
		string bindToField;

		public string[] DataNavigateUrlFields
		{
			get { return dataNavigateUrlFields; }
			set { dataNavigateUrlFields = value; }
		}
		string[] dataNavigateUrlFields;

		public string DataNavigateUrlFormatString
		{
			get { return dataNavigateUrlFormatString; }
			set { dataNavigateUrlFormatString = value; }
		}
		string dataNavigateUrlFormatString;

		public string Target
		{
			get { return target; }
			set { target = value; }
		}
		string target;

		public int? MaxItemsCount
		{
			get { return maxItemsCount; }
			set { maxItemsCount = value; }
		}
		int? maxItemsCount;

		#endregion
	}
}
