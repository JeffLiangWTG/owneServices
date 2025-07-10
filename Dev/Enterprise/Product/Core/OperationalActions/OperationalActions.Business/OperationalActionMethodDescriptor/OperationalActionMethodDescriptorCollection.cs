using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionMethodDescriptorCollection : NonPersistentBusinessObjectCollection<OperationalActionMethodDescriptor>, IXmlSerializable
	{
		#region Schema

		public static class Schema
		{
			public const string XmlElementName = "OperationalActionMethodDescriptor";
			public const string XmlCollectionName = "OperationalActionMethodDescriptorCollection";
		}

		#endregion

		public OperationalActionMethodDescriptorCollection(OperationalAction action)
			: base(action.Factory)
		{
			this.action = action;
		}

		public void LoadFromBlob(byte[] value)
		{
			using (SuspendListChanged())
			{
				RemoveAllButLeaveRelationshipsIntact();
				if (value.Length > 0)
				{
					using (MemoryStream stream = new MemoryStream(value))
					using (XmlTextReader reader = new XmlTextReader(stream))
					{
						reader.WhitespaceHandling = WhitespaceHandling.None;
						reader.ReadToFollowing(Schema.XmlCollectionName);
						((IXmlSerializable)this).ReadXml(reader);
					}
				}
			}
		}

		public byte[] SaveToBlob()
		{
			using (MemoryStream stream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement(Schema.XmlCollectionName);
				((IXmlSerializable)this).WriteXml(writer);
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
				return stream.ToArray();
			}
		}

		public OperationalActionMethodDescriptor AddNew(ActionMethodProviderID id, ZGuid methodID)
		{
			OperationalActionMethodDescriptor descriptor = AddNew();
			descriptor.MethodGroup = id.Guid;
			descriptor.MethodID = methodID;
			return descriptor;
		}

		public void SortByOrder()
		{
			Sort(OperationalActionMethodDescriptor.Schema.Order);
		}

		#region Collection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OperationalActionMethodDescriptor(action);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			unchecked
			{
				((OperationalActionMethodDescriptor)child).Order = (ZByte)(HighestOrder() + 1);
			}
		}

		internal IEnumerable<OperationalActionMethodDescriptor> FindElementsByOrder(byte order)
		{
			foreach (var element in Elements)
			{
				if (element is OperationalActionMethodDescriptor method && method.Order == order)
				{
					yield return method;
				}
			}
		}

		internal byte HighestOrder()
		{
			byte result = 0;
			foreach (var element in Elements)
			{
				if (element is OperationalActionMethodDescriptor method && method.Order > result)
				{
					result = method.Order;
				}
			}
			return result;
		}

		protected override bool AllowNewCore
		{
			get { return !OperationalActionMenuEditableHelper.ReadOnly(action); }
		}

		protected override bool AllowRemoveCore
		{
			get { return !OperationalActionMenuEditableHelper.ReadOnly(action); }
		}

		#endregion

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			if (reader.IsEmptyElement)
			{
				reader.ReadStartElement(Schema.XmlCollectionName);
			}
			else
			{
				reader.ReadStartElement(Schema.XmlCollectionName);
				while (reader.IsStartElement(Schema.XmlElementName))
				{
					OperationalActionMethodDescriptor methodDescriptor = AddNew();
					using (methodDescriptor.SuspendSettingHasChanges())
					{
						((IXmlSerializable)methodDescriptor).ReadXml(reader);
					}
				}
				reader.ReadEndElement();
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			foreach (IXmlSerializable methodDescriptor in this)
			{
				writer.WriteStartElement(Schema.XmlElementName);
				methodDescriptor.WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion

		readonly OperationalAction action;
	}
}
