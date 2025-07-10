using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class SIMADumpingNumber : NonPersistentBusinessObject
	{
		public SIMADumpingNumber(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema
		public static class Schema
		{
			public const string CA_DumpingNumber = "CA_DumpingNumber";
			public const string CA_DumpingDescription = "CA_DumpingDescription";
		}
		#endregion

		[ReadOnly(true)]
		[ResourceStringData("SIMADumpingNumber|CA_DumpingNumber", Caption = "Dumping Case#")]
		public ZString CA_DumpingNumber
		{
			get { return fCA_DumpingNumber; }
			set
			{
				fCA_DumpingNumber = value;
				CA_DumpingNumberInfo.RefreshBinding();
			}
		}
		ZString fCA_DumpingNumber;

		public ZPropertyInfo CA_DumpingNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CA_DumpingNumber); }
		}

		[ReadOnly(true)]
		[ResourceStringData("SIMADumpingNumber|CA_DumpingDescription", Caption = "Description")]
		public ZString CA_DumpingDescription
		{
			get { return fCA_DumpingDescription; }
			set
			{
				fCA_DumpingDescription = value;
				CA_DumpingDescriptionInfo.RefreshBinding();
			}
		}
		ZString fCA_DumpingDescription;

		public ZPropertyInfo CA_DumpingDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CA_DumpingDescription); }
		}
	}
}
