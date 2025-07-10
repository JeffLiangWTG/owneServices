using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[DependentBusinessObject(typeof(CusSeaManArrivalPort), "CargoLines")]
	public class CusSeaManOBLHeaderCargoLine : BaseCusSeaManOBLHeader, ICMRMessageRespondee
	{
		public CusSeaManOBLHeaderCargoLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		#pragma warning disable IDE0001 // Prevent simplification to base class
		public new abstract class Schema : Customs.Business.CusSeaManOBLHeader.Schema
		#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public const string CargoType = "CargoType";
			public const string CargoIdentifier = "CargoIdentifier";
			public const string PackageType = "PackageType";
			public const string NumberOfPackages = "NumberOfPackages";
		}

		#endregion

		#region Properties

		#region ArrivalPort

		public CusSeaManArrivalPort Port
		{
			get { return (CusSeaManArrivalPort)Factory.Load(typeof(CusSeaManArrivalPort), BO_BA); }
		}

		public override ZGuid BO_BA
		{
			get
			{
				return base.BO_BA;
			}
			set
			{
				Details.MarkAsNeedingValidationIncludingChildren();
				base.BO_BA = value;
			}
		}

		#endregion

		#region Detail

		public CusSeaManOBLDetailCargoLine Detail
		{
			get
			{
				if (fDetail == null)
				{
					if (Details.Count == 0)
					{
						fDetail = Details.AddNew();
					}
					else
					{
						fDetail = Details[0];
					}
					RegisterEditableChildObject(fDetail);
				}

				return fDetail;
			}
		}
		CusSeaManOBLDetailCargoLine fDetail;

		#endregion

		#endregion

		#region Proxied Properties

		#region CargoType

		public ZString CargoType
		{
			get { return Detail.BD_LineCargoType; }
			set { Detail.BD_LineCargoType = value; }
		}

		public ZPropertyInfo CargoTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CargoType, x => Detail.BD_LineCargoTypeInfo); }
		}

		#endregion

		#region	CargoIdentifier

		public ZString CargoIdentifier
		{
			get { return Detail.BD_ContainerNumber; }
			set { Detail.BD_ContainerNumber = value; }
		}

		public ZPropertyInfo CargoIdentifierInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CargoIdentifier, x => Detail.BD_ContainerNumberInfo); }
		}

		#endregion

		#region PackageType

		public ZString PackageType
		{
			get { return Detail.BD_PackType; }
			set { Detail.BD_PackType = value; }
		}

		public ZPropertyInfo PackageTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.PackageType, x => Detail.BD_PackTypeInfo); }
		}

		#endregion

		#region NumberOfPackages

		public ZInt NumberOfPackages
		{
			get { return Detail.BD_NoOfPacks; }
			set { Detail.BD_NoOfPacks = value; }
		}

		public ZPropertyInfo NumberOfPackagesInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.NumberOfPackages, x => Detail.BD_NoOfPacksInfo); }
		}

		#endregion

		#endregion

		#region Overrides

		#region Properties

		#region BO_HeaderCargoType

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderCargoLineLookups.CargoCodes))]
		public override ZString BO_HeaderCargoType
		{
			get { return base.BO_HeaderCargoType; }
			set
			{
				base.BO_HeaderCargoType = value;
				if (value == CMRCargoCodes.Codes.Empty)
				{
					CargoType = CMRCargoTypes.Codes.FullContainerLoad;
				}
			}
		}

		#endregion

		#region BO_RL_NKDischargePort

		public override ZString BO_RL_NKDischargePort
		{
			get { return base.BO_RL_NKDischargePort; }
			set
			{
				if (BO_RL_NKDischargePort.IsEmpty && BO_RL_NKDestinationPort.IsEmpty)
				{
					BO_RL_NKDestinationPort = value;
				}
				base.BO_RL_NKDischargePort = value;
			}
		}

		#endregion

		#region Details

		protected override Customs.Business.CusSeaManOBLDetailCollection GetNewDetails() => new CusSeaManOBLDetailCargoLineCollection(this);

		[ChildEditable(true)]
		public new CusSeaManOBLDetailCargoLineCollection Details => (CusSeaManOBLDetailCargoLineCollection)base.Details;

		#endregion

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BO_HeaderCargoType = CMRCargoCodes.Codes.Empty;
		}

		protected override Customs.Business.CusSeaManOBLHeaderLookups GetNewLookups() => new CusSeaManOBLHeaderCargoLineLookups(this);

		public new CusSeaManOBLHeaderCargoLineLookups Lookups => (CusSeaManOBLHeaderCargoLineLookups)base.Lookups;

		protected override Customs.Business.CusSeaManOBLHeaderValidation GetNewValidation() => new CusSeaManOBLHeaderCargoLineValidation(this);

		public new CusSeaManOBLHeaderCargoLineValidation Validation => (CusSeaManOBLHeaderCargoLineValidation)base.Validation;

		protected override ZString HumanReadableNameCore => "Cargo List" + (CargoIdentifier.IsEmpty ? "" : " " + CargoIdentifier);

		#region ICMRMessageRespondee

		ZString ICMRMessageRespondee.Details => ZString.Format("{0}Cargo List: {1}\r\n", ((ICMRMessageRespondee)TransportHeader).Details, CargoIdentifier);

		ZString ICMRMessageRespondee.ShortDescription => ZString.Format("{0} Cargo List: {1}", ((ICMRMessageRespondee)TransportHeader).ShortDescription, CargoIdentifier);

		EDIMessageCollection ICMRMessageRespondee.Messages => base.Messages;

		#endregion

		#endregion
	}
}
