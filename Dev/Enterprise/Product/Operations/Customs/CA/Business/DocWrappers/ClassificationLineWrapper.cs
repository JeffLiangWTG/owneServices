using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ClassificationLine1Wrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string B3LineNumber = "B3LineNumber";
			public const string B3SubHeaderNumber = "B3SubHeaderNumber";
			public const string ClassificationNumber = "ClassificationNumber";
			public const string ValueForDutyCode = "ValueForDutyCode";
			public const string TariffCode = "TariffCode";
			public const string AuthorityNumber = "AuthorityNumber";
			public const string ValueForCurrency = "ValueForCurrency";
			public const string IsSelected = "IsSelected";
		}

		#endregion

		public ClassificationLine1Wrapper(IClassificationLine1 classificationLine)
		{
			this.ClassificationLine = classificationLine;
		}
		public readonly IClassificationLine1 ClassificationLine;

		#region B3LineNumber

		[ResourceStringData("ebcb8704-59e2-4043-ad64-b6c6b930bcfc", Caption = "B3 Line Number")]
		public ZInt B3LineNumber
		{
			get { return ClassificationLine.B3LineNumber; }
		}

		public ZPropertyInfo B3LineNumberInfo
		{
			get { return GetZPropertyInfo(Schema.B3LineNumber); }
		}

		#endregion

		#region B3SubHeaderNumber

		[ResourceStringData("5336b91f-c074-4bb6-aed8-9e6c43bcb5de", Caption = "B3 Sub-Header Number")]
		public ZInt B3SubHeaderNumber
		{
			get { return ClassificationLine.B3SubHeaderNumber; }
		}

		public ZPropertyInfo B3SubHeaderNumberInfo
		{
			get { return GetZPropertyInfo(Schema.B3SubHeaderNumber); }
		}

		#endregion

		#region ClassificationNumber

		[ResourceStringData("6c93eee1-1759-4172-8a34-0138bf308064", Caption = "Classification Number")]
		public ZString ClassificationNumber
		{
			get { return ClassificationLine.ClassificationNumber; }
		}

		public ZPropertyInfo ClassificationNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ClassificationNumber); }
		}

		#endregion

		#region ValueForDutyCode

		[ResourceStringData("336beb7c-c629-4550-b622-873c19fde31e", Caption = "Value For Duty Code")]
		public ZString ValueForDutyCode
		{
			get { return ClassificationLine.ValueForDutyCode; }
		}

		public ZPropertyInfo ValueForDutyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ValueForDutyCode); }
		}

		#endregion

		#region TariffCode

		[ResourceStringData("0c4137fd-3d84-4e1d-9239-306269747856", Caption = "Tariff Code")]
		public ZString TariffCode
		{
			get { return ClassificationLine.TariffCode; }
		}

		public ZPropertyInfo TariffCodeInfo
		{
			get { return GetZPropertyInfo(Schema.TariffCode); }
		}

		#endregion

		#region AuthorityNumber

		[ResourceStringData("a8573fee-2f4e-4a9b-8457-01a93273300a", Caption = "Authority Number")]
		public ZString AuthorityNumber
		{
			get { return ClassificationLine.AuthorityNumber; }
		}

		public ZPropertyInfo AuthorityNumberInfo
		{
			get { return GetZPropertyInfo(Schema.AuthorityNumber); }
		}

		#endregion

		#region ValueForCurrency

		[ResourceStringData("b30d7320-8d11-45e6-9092-4ea4f04c40c6", Caption = "Value For Currency")]
		public ZDecimal ValueForCurrency
		{
			get { return ClassificationLine.ValueForCurrency; }
		}

		public ZPropertyInfo ValueForCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.ValueForCurrency); }
		}

		#endregion

		#region IsSelected

		public ZBool IsSelected
		{
			get { return fIsSelected; }
			set { SetNonPersistentPropertyValue(IsSelectedInfo, ref fIsSelected, value); }
		}
		ZBool fIsSelected;

		public ZPropertyInfo IsSelectedInfo
		{
			get { return GetZPropertyInfo(Schema.IsSelected); }
		}

		#endregion
	}
}
