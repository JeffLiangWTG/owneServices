using System.Globalization;
#if NETFRAMEWORK
using System.Web.Security.AntiXss;
#else
using System.Text.Encodings.Web;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class DocMyAccountWebContract : DocumentWrapper
	{
		DocMyAccountWebContract(MyAccountWebContract objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		public static DocMyAccountWebContract New(MyAccountWebContract objectToWrap, BusinessObjectFactory factory)
		{
			return new DocMyAccountWebContract(objectToWrap, factory);
		}

		public new MyAccountWebContract WrappedObject
		{
			get { return (MyAccountWebContract)base.WrappedObject; }
		}

		#region Logged In Organisation

		[DocumentField("The client's full name")]
		public ZString ClientName
		{
			get
			{
#if NETFRAMEWORK
				return AntiXssEncoder.HtmlEncode(AntiXssEncoder.HtmlEncode(WrappedObject.LoggedInOrganisation.OH_FullNameTruncated, false), false);
#else
				return HtmlEncoder.Default.Encode(HtmlEncoder.Default.Encode(WrappedObject.LoggedInOrganisation.OH_FullNameTruncated));
#endif
			}
		}

		[DocumentField("The client's main address (single line)")]
		public ZString ClientMainAddressInSingleLine
		{
			get { return WrappedObject.LoggedInOrganisation.MainAddress.AddressAsASingleLine; }
		}

		[DocumentField("The client's main address (multiple lines)")]
		public ZString ClientMainAddressInHTMLWithoutCompanyName
		{
			get
			{
				ZString address = new AddressFormatter(Factory, WrappedObject.LoggedInOrganisation.MainAddress, GlbCompany.CurrentCompany, false).PostalAddressWithoutCompanyName();
				return address.Replace("\n", "<br />");
			}
		}

		#endregion

		#region Logged In Contact

		[DocumentField("The contact's full name")]
		public ZString ContactName
		{
			get
			{
#if NETFRAMEWORK
			return AntiXssEncoder.HtmlEncode(AntiXssEncoder.HtmlEncode(WrappedObject.LoggedInContact.OC_ContactName, false), false);
#else
				return HtmlEncoder.Default.Encode(HtmlEncoder.Default.Encode(WrappedObject.LoggedInContact.OC_ContactName));
#endif
			}
		}

		#endregion

		#region Dates

		[DocumentField("Today's date in short date format eg 17/02/2011")]
		public ZString ShortDate
		{
			get { return ZDateTime.Now.ToShortDateString(); }
		}

		[DocumentField("Today's date in long date format eg Thursday, 17 Februrary 2011")]
		public ZString LongDate
		{
			get { return ZDateTime.Now.ToDateTime().ToLongDateString(); }
		}

		[DocumentField("Today's date in long date format, without the day eg, 17 Februrary 2011")]
		public ZString LongDateNoDay
		{
			get { return ZDateTime.Now.ToDateTime().ToString("dd MMMM yyyy", CultureInfo.CurrentCulture); }
		}

		#endregion

		#region Current Company

		[DocumentField("Current login company's name")]
		public ZString CurrentCompanyName
		{
			get { return GlbCompany.CurrentCompany.GC_Name; }
		}

		#endregion

		#region Web Contract Version

		[DocumentField("Version number of the web contract (DO NOT USE THIS IN VERSION SECTION)")]
		public ZString WebContractVersion
		{
			get { return WrappedObject.WebContractVersion; }
		}

		#endregion
	}
}

