using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class StlRawUsageReportRefCaptionCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new StlRawUsageReportRefCaption this[int index]
		{
			get { return (StlRawUsageReportRefCaption)Elements[index]; }
		}

		#region New

		public new StlRawUsageReportRefCaption AddNew()
		{
			return (StlRawUsageReportRefCaption)base.AddNew();
		}

		public StlRawUsageReportRefCaption AddNew(string usageCode, string usageDescription, string ref1Caption, string ref2Caption = "", string ref3Caption = "", string ref4Caption = "", string ref5Caption = "")
		{
			var result = AddNew();
			using (result.GetValidationSuspender())
			{
				result.UsageCode = usageCode;
				result.UsageDescription = usageDescription;
				result.Ref1Caption = ref1Caption;
				result.Ref2Caption = ref2Caption;
				result.Ref3Caption = ref3Caption;
				result.Ref4Caption = ref4Caption;
				result.Ref5Caption = ref5Caption;
			}

			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StlRawUsageReportRefCaption();
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StlRawUsageReportRefCaptionCollection();
		}

		#endregion
	}
}

