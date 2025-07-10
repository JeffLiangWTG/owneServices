using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("DescriptionValue")]
	public class InterchangeSenderProxyUser : RegistryBusinessObject
	{
		public InterchangeSenderProxyUser()
		{
		}

		public InterchangeSenderProxyUser(BusinessObjectFactory factory)
		 : base(factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new InterchangeSenderProxyUser(CurrentFactory);

		protected override int CodeMaxLengthDefaultValue => EDIInterchangeSchema.EI_From.MaxLength;

		protected override int MaxDescriptionLength => GlbStaffSchema.GS_Code.MaxLength;

		[ResourceStringData("InterchangeSenderProxyUser.Code", Caption = "Sender Code", FullDescription = "Specifies which sender will use the assigned staff code.")]
		public override ZString Code
		{
			get => base.Code;
			set => base.Code = value;
		}

		[List("StaffCollection")]
		[MaxLength(nameof(Description_MaxLength))]
		[ResourceStringData("InterchangeSenderProxyUser.DescriptionValue", Caption = "Staff Code", FullDescription = "The staff member that will be used when processing messages from the Recipient.")]
		public ZString DescriptionValue
		{
			get => base.Description;
			set
			{
				base.Description = (NoResString)value;
				DescriptionValueInfo.RefreshBinding();
				ValidateDescriptionValue();
			}
		}

		public ZPropertyInfo DescriptionValueInfo
		{
			get => GetZPropertyInfo(nameof(DescriptionValue));
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDescriptionValue();
		}

		public void ValidateDescriptionValue()
		{
			if (CurrentFactory != null)
			{
				DescriptionValueInfo.ClearAllNotifications();
				var staff = DescriptionValue.IsEmpty ? null : CurrentFactory.LoadTop1<IGlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, DescriptionValue));
				if (staff == null)
				{
					DescriptionValueInfo.AddError(ListValidation.GetNotificationMessage(DescriptionValueInfo.HumanReadableName.ToString()).ToString());
				}
			}
		}

		public IActiveBusinessObjectCollection StaffCollection => CurrentFactory.GetCachedValue("InterchangeSenderProxyUser.StaffCollection", () => (IActiveBusinessObjectCollection)ObjectFactory.Get<IGlbStaffCollection>("IGlbStaffCollection", CurrentFactory));
	}
}
