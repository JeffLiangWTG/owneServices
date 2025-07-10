using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionMethodDescriptor : AutoOperationalActionMethodDescriptor
	{
		public OperationalActionMethodDescriptor(OperationalAction action)
			: base(action.Factory)
		{
			if (action.Context == null)
			{
				throw new InvalidOperationException("action.Context cannot be null");
			}

			this.action = action;
			this.context = action.Context;
		}

		public OperationalActionContext Context
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return context; }
		}

		#region Related BusinessObjects

		public OperationalAction Action
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return action; }
		}

		public OperationalActionMethodSettings Settings
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (settings == null && Method != null && Method.HasSettings)
				{
					settings = Method.NewSetting(Factory);
					RegisterEditableChildObject(settings);
				}
				return settings;
			}
		}

		#endregion

		#region BusinessObject Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public override void Delete()
		{
			foreach (BusinessObjectCollection collection in ParentCollections)
			{
				((IBusinessObjectCollectionInternals)collection).HasChangesFromDelete = true;
			}
			base.Delete();
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || OperationalActionMenuEditableHelper.ReadOnly(action); }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.ReadOnly = value; }
		}

		#endregion

		#region Strategy Objects

		public OperationalActionMethodDescriptorLookups Lookups
		{
			get { return lookups ?? (lookups = new OperationalActionMethodDescriptorLookups(this)); }
		}

		#endregion

		#region Method

		public OperationalActionMethod Method
		{
			get { return method ?? (method = Context.Supporter.Methods.GetOperationalActionMethod(MethodGroup, MethodID)); }
		}

		void MethodChanged()
		{
			method = null;

			if (settings != null)
			{
				UnRegisterEditableChildObject(settings);
				settings = null;
			}
		}

		#endregion

		#region Bound Properties

		[List("Lookups.MethodGroups")]
		public override ZGuid MethodGroup
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.MethodGroup; }
			set
			{
				if (base.MethodGroup == value)
				{
					base.MethodGroup = value;
				}
				else
				{
					MethodChanged();
					base.MethodGroup = value;
					CodeDescriptionPairList list = Lookups.MethodNames;
					MethodID = list.Count > 0 ? (ZGuid)list[0].PK : ZGuid.Empty;
				}
			}
		}

		public override ZString MethodGroupName
		{
			get
			{
				ActionMethodProviderID id = ActionMethodProviderIDs.FindByGuid(MethodGroup);
				return id == null ? string.Empty : id.Name;
			}
		}

		public override ZString MethodName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				foreach (ICodeDescription pair in Lookups.MethodNames)
				{
					if (pair.PK.Equals(MethodID))
					{
						return pair.Code;
					}
				}
				return "";
			}
		}

		[List("Lookups.MethodNames")]
		public override ZGuid MethodID
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.MethodID; }
			set
			{
				if (base.MethodID != value)
				{
					if (!IsDeserialising)
					{
						Action.EnsureFiltersForMethod(Context.Supporter.Methods.GetOperationalActionMethod(MethodGroup, value));
					}
					MethodChanged();
				}

				base.MethodID = value;
			}
		}

		protected override bool MethodID_ReadOnly
		{
			get { return Lookups.MethodNames.Count == 0; }
		}

		public override ZByte Order
		{
			get => base.Order;

			set
			{
				if (value != base.Order)
				{
					var oldOrder = base.Order;
					base.Order = value;

					foreach (var collection in ParentCollections)
					{
						if (collection is OperationalActionMethodDescriptorCollection operationalActionMethodDescriptorCollection)
						{
							foreach (var method in operationalActionMethodDescriptorCollection.FindElementsByOrder(oldOrder))
							{
								((IBusinessObjectInternals)method).Validate(OperationalActionFieldDescriptor.Schema.Order);
							}
							foreach (var method in operationalActionMethodDescriptorCollection.FindElementsByOrder(value))
							{
								((IBusinessObjectInternals)method).Validate(OperationalActionFieldDescriptor.Schema.Order);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadSettings(XmlReader reader)
		{
			if (Settings == null || reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				using (Settings.GetValidationSuspender())
				using (Settings.SuspendSettingHasChanges())
				{
					((IXmlSerializable)Settings).ReadXml(reader);
				}
			}
		}

		protected override void WriteSettings(XmlWriter writer)
		{
			if (Settings != null)
			{
				((IXmlSerializable)Settings).WriteXml(writer);
			}
		}

		#endregion

		OperationalActionMethodDescriptorLookups lookups;
		OperationalActionMethodSettings settings;
		OperationalActionMethod method;
		readonly OperationalAction action;
		readonly OperationalActionContext context;
	}
}
