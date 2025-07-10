using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.eHub
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BillingTransactionsTransformationSettings : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string CurrentFixIndex = "CurrentFixIndex";
			public const string CurrentFixState = "CurrentFixState";
		}

		#endregion

		#region Properties

		#region CurrentFixIndex

		public ZInt CurrentFixIndex
		{
			get { return currentFixIndex; }
			set { SetNonPersistentPropertyValue(CurrentFixIndexInfo, ref currentFixIndex, value); }
		}
		ZInt currentFixIndex;

		public ZPropertyInfo CurrentFixIndexInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentFixIndex); }
		}

		#endregion // CurrentFixIndex

		#region CurrentFixState

		public ZInt CurrentFixState
		{
			get { return currentFixState; }
			set { SetNonPersistentPropertyValue(CurrentFixStateInfo, ref currentFixState, value); }
		}
		ZInt currentFixState;

		public ZPropertyInfo CurrentFixStateInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentFixState); }
		}

		#endregion // CurrentFixState

		#endregion // Properties

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new BillingTransactionsTransformationSettings
			{
				CurrentFixIndex = CurrentFixIndex,
				CurrentFixState = CurrentFixState
			};
			return result;
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var currentFixIndexString = reader.ReadElementString(Schema.CurrentFixIndex);
			CurrentFixIndex = string.IsNullOrEmpty(currentFixIndexString)
				? new ZInt(0)
				: new ZInt(currentFixIndexString);

			var currentFixStateString = reader.ReadElementString(Schema.CurrentFixState);
			CurrentFixState = string.IsNullOrEmpty(currentFixStateString)
				? new ZInt(0)
				: new ZInt(currentFixStateString);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CurrentFixIndex, CurrentFixIndex.ToString());
			writer.WriteElementString(Schema.CurrentFixState, CurrentFixState.ToString());
		}

		#endregion // Override
	}
}
