using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Column responsible for displaying hyperlinks to detailed objects from the grid
	/// </summary>
	public class ZHyperLinkColumn : ZTemplateColumn
	{
		public ZHyperLinkColumn(string headerText, string bindTo) : base(headerText, bindTo)
		{
			this.HeaderText = headerText;

			fIsExternalHyperlink = false;

			fText = "";
			DataTextFields = InitializeArrayFromBindTo();
			DataTextFormatString = "{0}";

			fNavigateUrl = "";
			fDataNavigateUrlFormatString = "";
			fDataNavigateUrlFields = InitializeArrayFromBindTo();

			fImageUrl = "";
			fDataImageUrlFormatString = "";
			fDataImageUrlFields = System.Array.Empty<string>();
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZHyperLinkColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return GetItemTemplate();
		}

		#region ClientClickHandler

		public string ClientClickHandler
		{
			get { return fClientClickHandler; }
			set { fClientClickHandler = value; }
		}

		string fClientClickHandler;

		#endregion

		#region Presentation

		/// <summary>
		/// The DataTextField and Text properties cannot both be set at the same time. 
		/// If both properties are set, the DataTextField property takes precedence.
		/// </summary>

		/// <summary>
		/// Text in the column.
		/// </summary>
		public string Text
		{
			get { return fText; }
			set { fText = value; }
		}
		string fText;

		/// <summary>
		/// Dynamic component of Text, contains field names in the Business object the row is bound to
		/// values are taken from all of those fields and added into the Text from DataTextFormatString
		/// </summary>
		public string[] DataTextFields
		{
			get { return fDataTextFields; }
			set { fDataTextFields = value; }
		}
		string[] fDataTextFields;

		/// <summary>
		/// Static Url to Image file. Every row in the grid will have the same image
		/// </summary>
		public string ImageUrl
		{
			get { return fImageUrl; }
			set { fImageUrl = value; }
		}
		string fImageUrl;

		/// <summary>
		/// Dynamic component of ImageUrl, contains field names in the Business object the row is bound to
		/// values are taken from all of those fields and added into the ImageUrl from DataImageUrlFormatString
		/// </summary>
		public string[] DataImageUrlFields
		{
			get { return fDataImageUrlFields; }
			set { fDataImageUrlFields = value; }
		}
		string[] fDataImageUrlFields;

		#endregion

		#region Url navigation

		/// <summary>
		/// The DataNavigateUrlFields and NavigateUrl properties cannot both be set at the same time. 
		/// If both properties are set, the DataNavigateUrlFields property takes precedence.
		/// </summary>

		/// <summary>
		/// Static Url to navigate to. Every row in the grid will point to the same Url
		/// </summary>
		public string NavigateUrl
		{
			get { return fNavigateUrl; }
			set { fNavigateUrl = value; }
		}
		string fNavigateUrl;

		/// <summary>
		/// Dynamic component of Url, contains field names in the Business object the row is bound to
		/// values are taken from all of those fields and added into the Url from DataNavigateUrlFormatString
		/// </summary>
		public string[] DataNavigateUrlFields
		{
			get { return fDataNavigateUrlFields; }
			set { fDataNavigateUrlFields = value; }
		}
		string[] fDataNavigateUrlFields;

		public string Target
		{
			get { return fTarget; }
			set { fTarget = value; }
		}
		string fTarget;

		public bool IsExternalHyperlink
		{
			get { return fIsExternalHyperlink; }
			set { fIsExternalHyperlink = value; }
		}

		bool fIsExternalHyperlink;

		public string WindowStyle
		{
			get { return fWindowStyle; }
			set { fWindowStyle = value; }
		}
		string fWindowStyle;

		#endregion

		#region Formatting

		/// <summary>
		/// Format string for Url that user go to when they click on the text in this column. 
		/// This value is dynamically combined with DataNavigateUrlFields
		/// </summary>
		public string DataNavigateUrlFormatString
		{
			get { return fDataNavigateUrlFormatString; }
			set { fDataNavigateUrlFormatString = value; }
		}
		string fDataNavigateUrlFormatString;

		/// <summary>
		/// Format string to specify Text of the cell in each row of the grid
		/// This value is dynamically combined with DataTextFields
		/// </summary>
		public string DataTextFormatString
		{
			get { return fDataTextFormatString; }
			set { fDataTextFormatString = value; }
		}
		string fDataTextFormatString;

		/// <summary>
		/// Format string to specify ImageUrl of the cell in each row of the grid
		/// This value is dynamically combined with DataImageUrlFields
		/// </summary>
		public string DataImageUrlFormatString
		{
			get { return fDataImageUrlFormatString; }
			set { fDataImageUrlFormatString = value; }
		}
		string fDataImageUrlFormatString;

		#endregion

		string[] InitializeArrayFromBindTo()
		{
			return string.IsNullOrEmpty(BindTo) ? System.Array.Empty<string>() : new string[1] { BindTo };
		}
	}
}
