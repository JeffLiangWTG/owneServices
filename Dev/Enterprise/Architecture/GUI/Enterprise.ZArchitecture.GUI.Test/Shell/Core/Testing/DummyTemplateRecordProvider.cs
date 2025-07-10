using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[PreventDelete(true)]
	public sealed class DummyTemplateRecordProvider : DummyBusinessObject, ITemplateRecordProvider, ICancellable
	{
		public DummyTemplateRecordProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region ICancellable Members

		public string CanCancel()
		{
			return null;
		}

		public string CanReactivate()
		{
			return null;
		}

		public bool IsCancelled
		{
			get
			{
				return Z0_Bool;
			}
			set
			{
				Z0_Bool = value;
			}
		}

		public bool IsCancelledHasChanged
		{
			get { return false; }
		}

		#endregion

		#region ITemplateRecordProvider Members

		public void SaveToTemplateRecord()
		{
		}

		public void LoadFromTemplateRecord(ITemplateRecord templateRecord)
		{
		}

		public BusinessObject InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord templateRecord)
		{
			throw new NotImplementedException();
		}

		public bool IsTemplateRecord
		{
			get
			{
				return isTemplateRecord;
			}
			set
			{
				isTemplateRecord = value;
			}
		}

		bool isTemplateRecord = true;

		public ITemplateRecord TemplateRecord
		{
			get
			{
				if (templateRecord != null)
				{
					return templateRecord;
				}

				return Factory.Load<DummyTemplateRecord>(Z0_Guid);
			}
			set
			{
				templateRecord = value;
				Z0_Guid = value != null && value is BusinessObject newValue
					? newValue.PK
					: ZGuid.Empty;
			}
		}

		ITemplateRecord templateRecord;

		#endregion
	}
}
