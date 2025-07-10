using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class CusPerson : ASYCUDA.Business.CusPerson
	{
		public CusPerson(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("AEFB267F-4E39-4466-A2F6-E8CA25B39057", Caption = "Address")]
		public ZString PersonAddress
		{
			get
			{
				if (Person is GlbPerson glbPerson)
				{
					return string.Join(" ", glbPerson.PER_HomeAddress1, glbPerson.PER_HomeAddress2);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		[ResourceStringData("0962C5F2-EAAF-44B6-B58A-7D2B696C7937", Caption = "Email")]
		public ZString PersonEmail => Person?.PER_EmailAddress ?? ZString.Empty;

		[ResourceStringData("4A208910-D6F4-45A1-AC2B-43FC2B9F52D3", Caption = "Home Phone")]
		public ZString PersonHomePhone => Person?.PER_HomePhone ?? ZString.Empty;

		[ResourceStringData("27033094-2ECD-4A26-8365-1F3092CCEED7", Caption = "Mobile Phone")]
		public ZString PersonMobilePhone => Person?.PER_MobilePhone ?? ZString.Empty;
	}
}
