using System;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.AU.Declaration.Business.XmlSerializers")]
	public class DefaultPremiseID : RegistryBusinessObjectTemplate
	{
		public DefaultPremiseID()
		{
		}

		public DefaultPremiseID(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string AirlineCode = "AirlineCode";
			public const string PortOfDischarge = "PortOfDischarge";
			public const string PremiseID = "PremiseID";
		}

		#endregion

		#region Overrides

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultPremiseID(factory);
		}

		#endregion

		#region WriteElements

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.AirlineCode, AirlineCode);
			writer.WriteElementString(Schema.PortOfDischarge, PortOfDischarge);
			writer.WriteElementString(Schema.PremiseID, PremiseID);
		}

		#endregion

		#region ReadElements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			AirlineCode = reader.ReadElementString(Schema.AirlineCode);
			PortOfDischarge = reader.ReadElementString(Schema.PortOfDischarge);
			PremiseID = reader.ReadElementString(Schema.PremiseID);
		}

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAirlineCode();
			ValidatePortOfDischarge();
			ValidatePremiseID();
		}

		#endregion

		#endregion

		#region Bound Properties

		#region AirlineCode

		ZString airlineCode;
		[MaxLength(2)]
		public ZString AirlineCode
		{
			get { return airlineCode; }
			set
			{
				CheckMaximumLength(AirlineCodeInfo, value);
				SetNonPersistentPropertyValue(AirlineCodeInfo, ref airlineCode, value);
				if (!IsValidationSuspended)
				{
					ValidateAirlineCode();
				}
			}
		}

		public ZPropertyInfo AirlineCodeInfo
		{
			get { return GetZPropertyInfo(Schema.AirlineCode); }
		}

		void ValidateAirlineCode()
		{
			AirlineCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AirlineCodeInfo);
		}

		#endregion

		#region PortOfDischarge

		ZString portOfDischarge;
		[MaxLength(5)]
		public ZString PortOfDischarge
		{
			get { return portOfDischarge; }
			set
			{
				CheckMaximumLength(PortOfDischargeInfo, value);
				SetNonPersistentPropertyValue(PortOfDischargeInfo, ref portOfDischarge, value);
				if (!IsValidationSuspended)
				{
					ValidatePortOfDischarge();
				}
			}
		}

		public ZPropertyInfo PortOfDischargeInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfDischarge); }
		}

		void ValidatePortOfDischarge()
		{
			PortOfDischargeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PortOfDischargeInfo);
			if (!PortOfDischargeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(PortOfDischargeInfo, UNLocoCollection);
			}
		}

		#endregion

		#region PremiseID

		ZString premiseID;
		[MaxLength(10)]
		public ZString PremiseID
		{
			get { return premiseID; }
			set
			{
				CheckMaximumLength(PremiseIDInfo, value);
				SetNonPersistentPropertyValue(PremiseIDInfo, ref premiseID, value);
				if (!IsValidationSuspended)
				{
					ValidatePremiseID();
				}
			}
		}

		public ZPropertyInfo PremiseIDInfo
		{
			get { return GetZPropertyInfo(Schema.PremiseID); }
		}

		void ValidatePremiseID()
		{
			PremiseIDInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PremiseIDInfo);
		}

		#endregion

		#endregion

		#region UNLocoCollection

		IBusinessObjectCollection fUNLocoCollection;
		public IBusinessObjectCollection UNLocoCollection
		{
			get
			{
				if (fUNLocoCollection == null)
				{
					Assembly assembly = Assembly.Load("Enterprise.MasterFiles.Business");
					Type refUNLOCOCollectionType = assembly.GetType("Enterprise.MasterFiles.Business.RefUNLOCOCollection");
					fUNLocoCollection = (IBusinessObjectCollection)Activator.CreateInstance(refUNLOCOCollectionType, new object[] { CurrentFactory });
				}
				return fUNLocoCollection;
			}
		}

		#endregion

		#region ReadOnlyFactory

		BusinessObjectFactory fReadOnlyFactory;
		protected BusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (fReadOnlyFactory == null)
				{
					fReadOnlyFactory = base.Factory ?? new BusinessObjectFactory();
				}
				return fReadOnlyFactory;
			}
		}

		#endregion
	}
}
