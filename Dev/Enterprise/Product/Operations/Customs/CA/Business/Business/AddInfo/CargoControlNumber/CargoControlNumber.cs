namespace Enterprise.Customs.CA.Business
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Data;
	using System.Globalization;
	using CargoWise.ComponentModel;
	using CargoWise.Customs.CA.MessageContracts.CAD;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Business.MultiLineAddInfos;
	using Enterprise.Customs.Common;
	using Enterprise.MasterFiles.Business.CustomValues;

	[SystemDefinedValues]
	[CodeProperty(Schema.CY_CargoControlNumber), DescriptionProperty(Schema.CY_CargoControlNumber)]
	public class CargoControlNumber : AutoCargoControlNumber, ISynchroniserReadOnlyMembersProvider, ICADMessageDeclarationAdditionalDocument, Integration.Customs.CA.ICargoControlNumber
	{
		public CargoControlNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCargoControlNumber.Schema
		{
			public const string CY_CargoControlNumber = "CY_CargoControlNumber";
			public const string CY_DateOfRelease = "CY_DateOfRelease";
			public const int CY_CargoControlNumberMaxLength = 25;
			public const int CY_CargoControlNumberPrefixLength = 4;
			public const int CY_CargoControlNumberSuffixMaxLength = 21;
		}

		public new JobDeclaration Parent { get => base.Parent as JobDeclaration; internal set => SetParent(value); }

		#region CY_CargoControlNumber

		[MaxLength(Schema.CY_CargoControlNumberMaxLength)]
		public ZString CY_CargoControlNumber
		{
			get
			{
				if (!isCCNLoaded)
				{
					cargoControlNumber = CA_CCNInfoNumber.Left(Schema.CY_CargoControlNumberMaxLength);
					isCCNLoaded = true;
				}
				return cargoControlNumber;
			}
			set
			{
				var shouldCarrierCodeBeSet = ShouldCarrierCodeBeSet();
				SetNonPersistentPropertyValue(CY_CargoControlNumberInfo, ref cargoControlNumber, value);
				SetToCA_CCNInfoNumber();
				SetCarrierCode(shouldCarrierCodeBeSet);
			}
		}

		public ZPropertyInfo CY_CargoControlNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CY_CargoControlNumber); }
		}

		ZString cargoControlNumber;
		bool isCCNLoaded;

		#endregion

		#region CY_DateOfRelease

		public ZDateTime CY_DateOfRelease
		{
			get
			{
				if (!isDateOfReleaseLoaded)
				{
					if (!ZDateTime.TryParseExact(CA_CCNInfoNumber.SubstringSafe(Schema.CY_CargoControlNumberMaxLength).TrimEnd(), out dateOfRelease, DateFormat))
					{
						dateOfRelease = ZDateTime.Empty;
					}
					isDateOfReleaseLoaded = true;
				}
				return dateOfRelease;
			}
			set
			{
				SetNonPersistentPropertyValue(CY_DateOfReleaseInfo, ref dateOfRelease, value);
				SetToCA_CCNInfoNumber();
			}
		}

		public ZPropertyInfo CY_DateOfReleaseInfo
		{
			get { return GetZPropertyInfo(Schema.CY_DateOfRelease); }
		}

		ZDateTime dateOfRelease;
		bool isDateOfReleaseLoaded;

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CA_CU_CCNInfoBill = ZGuid.Empty;
		}

		void SetToCA_CCNInfoNumber()
		{
			CA_CCNInfoNumber = CY_CargoControlNumber.PadRight(Schema.CY_CargoControlNumberMaxLength, ' ') + CY_DateOfRelease.ToString(DateFormat, CultureInfo.CurrentCulture);
		}

		#region SetCarrierCode

		bool ShouldCarrierCodeBeSet()
		{
			return Parent != null && (Parent.JE_CarrierCode.IsEmpty || Parent.JE_CarrierCode == CY_CargoControlNumber.SubstringSafe(0, 4));
		}

		void SetCarrierCode(bool shouldCarrierCodeBeSet)
		{
			if (shouldCarrierCodeBeSet && Parent.CargoControlNumbers.Count > 0 && Parent.CargoControlNumbers[0].PK == PK)
			{
				Parent.JE_CarrierCode = CY_CargoControlNumber.SubstringSafe(0, 4);
			}
		}

		#endregion

		const string DateFormat = "yyyyMMdd";

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new CargoControlNumberValidation(this);
		}

		#endregion

		#region ISynchroniserReadOnlyMembersProvider
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region ICADMessageDeclarationAdditionalDocument

		string ICADMessageDeclarationAdditionalDocument.ID
		{
			get
			{
				return CA_CCNInfoNumber;
			}
		}

		string ICADMessageDeclarationAdditionalDocument.TypeCode
		{
			get
			{
				return CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			}
		}

		#endregion
	}
}
