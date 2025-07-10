using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	[XmlRoot("DraftTransactionStatusReasonCode")]
	public class DraftTransactionStatusReasonCode : CodeDescription<ZBool>, ICanDelete
	{
		#region Schema

		protected new abstract class Schema : CodeDescription<ZBool>.Schema
		{
			public const string ANL = "ANL";
			public const string DFT = "DFT";
			public const string DSC = "DSC";
			public const string DIS = "DIS";
			public const string AFP = "AFP";
			public const string AWA = "AWA";
			public const string PRS = "PRS";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DraftTransactionStatusReasonCode();
		}

		#region ICanDelete

		public override bool CanDelete
		{
			get { return base.CanDelete && !IsDefaultReasonCode; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsDefaultReasonCode)
				{
					return ResString.GetMultilingualString("5A2DAB7F-0269-46E4-89D2-9942A8C0672B", "Cannot delete default reason code.");
				}
				return base.ReasonForNotAbleToDelete;
			}
		}

		#endregion

		bool IsDefaultReasonCode
		{
			get { return DefaultReasonCode == Code; }
		}

		protected override bool CodeAndDescriptionReadOnly => IsDefaultReasonCode;

		protected override int MaxDescriptionLength => 200;

		protected override bool IsCodeUniqueInCollection => true;

		protected override bool IsDescriptionMandatory => !IsDefaultReasonCode;

		#region Properties

		#region Code

		protected override void ValidateCodeCore()
		{
			base.ValidateCodeCore();

			if (Code.Length != 3 || !Code.IsLettersAndNumbersOnlyOrEmpty)
			{
				CodeInfo.AddError(ResString.GetMultilingualString("18A3470D-140F-46A2-BD71-F6B2EF1E6ED9", "Code must be 3 alphanumeric characters."));
			}
		}

		#endregion

		#region ANL

		ZBool fANL;

		public virtual ZBool ANL
		{
			get { return fANL; }
			set
			{
				if (fANL != value)
				{
					SetNonPersistentPropertyValue(ANLInfo, ref fANL, value);
				}
			}
		}

		public ZPropertyInfo ANLInfo
		{
			get { return GetZPropertyInfo(nameof(ANL)); }
		}

		#endregion

		#region DFT

		ZBool fDFT;

		public virtual ZBool DFT
		{
			get { return fDFT; }
			set
			{
				if (fDFT != value)
				{
					SetNonPersistentPropertyValue(DFTInfo, ref fDFT, value);
				}
			}
		}

		public ZPropertyInfo DFTInfo
		{
			get { return GetZPropertyInfo(nameof(DFT)); }
		}

		#endregion

		#region DSC

		ZBool fDSC;

		public virtual ZBool DSC
		{
			get { return fDSC; }
			set
			{
				if (fDSC != value)
				{
					SetNonPersistentPropertyValue(DSCInfo, ref fDSC, value);
				}
			}
		}

		public ZPropertyInfo DSCInfo
		{
			get { return GetZPropertyInfo(nameof(DSC)); }
		}

		#endregion

		#region DIS

		ZBool fDIS;

		public virtual ZBool DIS
		{
			get { return fDIS; }
			set
			{
				if (fDIS != value)
				{
					SetNonPersistentPropertyValue(DISInfo, ref fDIS, value);
				}
			}
		}

		public ZPropertyInfo DISInfo
		{
			get { return GetZPropertyInfo(nameof(DIS)); }
		}

		#endregion

		#region AFP

		ZBool fAFP;

		public virtual ZBool AFP
		{
			get { return fAFP; }
			set
			{
				if (fAFP != value)
				{
					SetNonPersistentPropertyValue(AFPInfo, ref fAFP, value);
				}
			}
		}

		public ZPropertyInfo AFPInfo
		{
			get { return GetZPropertyInfo(nameof(AFP)); }
		}

		#endregion

		#region AWA

		ZBool fAWA;

		public virtual ZBool AWA
		{
			get { return fAWA; }
			set
			{
				if (fAWA != value)
				{
					SetNonPersistentPropertyValue(AWAInfo, ref fAWA, value);
				}
			}
		}

		public ZPropertyInfo AWAInfo
		{
			get { return GetZPropertyInfo(nameof(AWA)); }
		}

		#endregion

		#region PRS

		ZBool fPRS;

		public virtual ZBool PRS
		{
			get { return fPRS; }
			set
			{
				if (fPRS != value)
				{
					SetNonPersistentPropertyValue(PRSInfo, ref fPRS, value);
				}
			}
		}

		public ZPropertyInfo PRSInfo
		{
			get { return GetZPropertyInfo(nameof(PRS)); }
		}

		#endregion

		#endregion

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);

			ANL = new ZBool(reader.ReadElementString(Schema.ANL));
			DFT = new ZBool(reader.ReadElementString(Schema.DFT));
			DSC = new ZBool(reader.ReadElementString(Schema.DSC));
			DIS = new ZBool(reader.ReadElementString(Schema.DIS));
			AFP = new ZBool(reader.ReadElementString(Schema.AFP));
			AWA = new ZBool(reader.ReadElementString(Schema.AWA));
			PRS = new ZBool(reader.ReadElementString(Schema.PRS));
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);

			writer.WriteElementString(Schema.ANL, ANL.ToString());
			writer.WriteElementString(Schema.DFT, DFT.ToString());
			writer.WriteElementString(Schema.DSC, DSC.ToString());
			writer.WriteElementString(Schema.DIS, DIS.ToString());
			writer.WriteElementString(Schema.AFP, AFP.ToString());
			writer.WriteElementString(Schema.AWA, AWA.ToString());
			writer.WriteElementString(Schema.PRS, PRS.ToString());
		}

		#region ParentCollection

		public DraftTransactionStatusReasonCodeCollection ParentCollection
		{
			get { return (DraftTransactionStatusReasonCodeCollection)GetParentCollection(this, typeof(DraftTransactionStatusReasonCodeCollection)); }
		}

		#endregion

		protected override ZBool ValueFromString(string value)
		{
			return new ZBool(value);
		}

		public const string DefaultReasonCode = "OTH";
	}
}
