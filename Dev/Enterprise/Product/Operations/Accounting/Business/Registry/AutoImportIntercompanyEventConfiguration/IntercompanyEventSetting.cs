using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class IntercompanyEventSetting : RegistryBusinessObjectTemplate
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new IntercompanyEventSetting();

			using (clone.GetValidationSuspender())
			{
				clone.StmEventCode = StmEventCode;
				clone.StartDate = StartDate;
			}

			return clone;
		}

		public BusinessObjectCollection ParentCollection
		{
			get { return GetParentCollection(this, typeof(IntercompanyEventSettingCollection)); }
		}

		#region Validation

		public IntercompanyEventValidation Validation => validation ?? (validation = new IntercompanyEventValidation(this));
		IntercompanyEventValidation validation;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateStmEventCode();
			Validation.ValidateStartDate();
		}

		#endregion

		[List(nameof(EventsList))]
		[ResourceStringData("IntercompanyEventSetting|StmEventCode", Caption = "Event")]
		public ZString StmEventCode
		{
			get
			{
				return stmEventCode;
			}
			set
			{
				if (stmEventCode != value)
				{
					SetNonPersistentPropertyValue(StmEventCodeInfo, ref stmEventCode, value);
					stmEvent = PrivateFactory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, stmEventCode);
					if (!IsValidationSuspended)
					{
						Validation.ValidateStmEventCode();
					}
				}
			}
		}
		ZString stmEventCode;
		StmEvent stmEvent;
		public ZPropertyInfo StmEventCodeInfo
		{
			get { return GetZPropertyInfo(nameof(StmEventCode)); }
		}

		BusinessObjectFactory PrivateFactory
		{
			get
			{
				if (factory == null)
				{
					factory = CreateNewFactory();
				}

				return factory;
			}
		}
		BusinessObjectFactory factory;

		[ResourceStringData("IntercompanyEventSetting|StmEventDescription", Caption = "Description")]
		public ZString StmEventDescription
		{
			get { return stmEvent == null ? ZString.Empty : stmEvent.SE_DescMultilingual; }
		}

		[ResourceStringData("IntercompanyEventSetting|StartDate", Caption = "Start Date")]
		public ZDate StartDate
		{
			get
			{
				return startDate;
			}
			set
			{
				SetNonPersistentPropertyValue(StartDateInfo, ref startDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateStartDate();
				}
			}
		}
		ZDate startDate;

		public ZPropertyInfo StartDateInfo
		{
			get { return GetZPropertyInfo(nameof(StartDate)); }
		}

		public StmEventCodeDescriptionPairList EventsList
		{
			get
			{
				if (eventsList == null)
				{
					eventsList = new StmEventCodeDescriptionPairList();
				}
				return eventsList;
			}
		}
		StmEventCodeDescriptionPairList eventsList;

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(nameof(StmEventCode), StmEventCode.ToString());
			writer.WriteElementString(nameof(StartDate), StartDate.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			StmEventCode = new ZString(reader.ReadElementString(nameof(StmEventCode)));
			StartDate = new ZDate(reader.ReadElementString(nameof(StartDate)));
		}

		#endregion
	}
}
