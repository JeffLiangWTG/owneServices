using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ExcludedFullyDigitalizedElectronicInvoiceData : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string BuyerAddress = "BuyerAddress";
			public const string BuyerPhoneNumber = "BuyerPhoneNumber";
			public const string BuyerBankAccount = "BuyerBankAccount";
		}

		#endregion Schema

		public ExcludedFullyDigitalizedElectronicInvoiceData()
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExcludedFullyDigitalizedElectronicInvoiceData();
		}

		public ZBool BuyerAddress
		{
			get => buyerAddress;
			set => SetNonPersistentPropertyValue(BuyerAddressInfo, ref buyerAddress, value);
		}

		ZBool buyerAddress;

		public ZPropertyInfo BuyerAddressInfo
		{
			get { return GetZPropertyInfo(Schema.BuyerAddress); }
		}

		public ZBool BuyerPhoneNumber
		{
			get => buyerPhoneNumber;
			set => SetNonPersistentPropertyValue(BuyerPhoneNumberInfo, ref buyerPhoneNumber, value);
		}

		ZBool buyerPhoneNumber;

		public ZPropertyInfo BuyerPhoneNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BuyerPhoneNumber); }
		}

		public ZBool BuyerBankAccount
		{
			get => buyerBankAccount;
			set => SetNonPersistentPropertyValue(BuyerBankAccountInfo, ref buyerBankAccount, value);
		}

		ZBool buyerBankAccount;

		public ZPropertyInfo BuyerBankAccountInfo
		{
			get { return GetZPropertyInfo(Schema.BuyerBankAccount); }
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.BuyerAddress, BuyerAddress.ToString());
			writer.WriteElementString(Schema.BuyerPhoneNumber, BuyerPhoneNumber.ToString());
			writer.WriteElementString(Schema.BuyerBankAccount, BuyerBankAccount.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			BuyerAddress = new ZBool(reader.ReadElementString(Schema.BuyerAddress));
			BuyerPhoneNumber = new ZBool(reader.ReadElementString(Schema.BuyerPhoneNumber));
			BuyerBankAccount = new ZBool(reader.ReadElementString(Schema.BuyerBankAccount));
		}
	}
}
