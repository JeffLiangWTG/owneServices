using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionFieldDescriptorCollection : NonPersistentBusinessObjectCollection<OperationalActionFieldDescriptor>
	{
		#region Schema

		public static class Schema
		{
			public const string Path = "path";
			public const string Code = "Code";

			public const string XmlCollectionName = "OperationalActionFieldDescriptorCollection";
			public const string XmlElementName = "OperationalActionFieldDescriptor";
			public const string XmlFilterElementName = "OperationalActionFieldCollectionFilter";
		}

		#endregion

		public OperationalActionFieldDescriptorCollection(OperationalAction action)
			: base(action.Factory)
		{
			this.action = action;
		}

		public void SortByOrder()
		{
			Sort(OperationalActionFieldDescriptor.Schema.Order);
		}

		internal IEnumerable<OperationalActionFieldDescriptor> FindElementsByOrder(byte order)
		{
			foreach (var element in Elements)
			{
				if (element is OperationalActionFieldDescriptor field && field.Order == order)
				{
					yield return field;
				}
			}
		}

		internal byte HighestOrder()
		{
			byte result = 0;
			foreach (var element in Elements)
			{
				if (element is OperationalActionFieldDescriptor field && field.Order > result)
				{
					result = field.Order;
				}
			}
			return result;
		}

		#region BusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OperationalActionFieldDescriptor(action);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			//might overflow in a full collection
			unchecked
			{
				((OperationalActionFieldDescriptor)child).Order = (byte)(HighestOrder() + 1);
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			var order = ((OperationalActionFieldDescriptor)bizO).Order;

			foreach (var field in FindElementsByOrder(order))
			{
				((IBusinessObjectInternals)field).Validate(OperationalActionFieldDescriptor.Schema.Order);
			}
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

		#region LoadFromBlob / SaveToBlob

		public void LoadFromBlob(byte[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

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

						if (!reader.IsEmptyElement)
						{
							reader.ReadStartElement(Schema.XmlCollectionName);
							ReadElements(reader);
							reader.ReadEndElement();
						}
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
				WriteElements(writer);
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
				return stream.ToArray();
			}
		}

		#endregion

		#region Read / Write Elements

		void ReadElements(XmlReader reader)
		{
			while (reader.IsStartElement(Schema.XmlElementName))
			{
				OperationalActionFieldDescriptor fieldDescriptor = AddNew();
				using (fieldDescriptor.SuspendSettingHasChanges())
				using (fieldDescriptor.GetValidationSuspender())
				{
					((IXmlSerializable)fieldDescriptor).ReadXml(reader);
				}
			}
		}

		void WriteElements(XmlWriter writer)
		{
			foreach (IXmlSerializable fieldDescriptor in this)
			{
				writer.WriteStartElement(Schema.XmlElementName);
				fieldDescriptor.WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion

		readonly OperationalAction action;
	}
}
