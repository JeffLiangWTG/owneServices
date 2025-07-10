using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyBizoWithITemplateRecordProvider : DummyWithRelatedLogs, ITemplateRecordProvider
	{
		public DummyBizoWithITemplateRecordProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			AutoLoggingState = EnterpriseBusinessObject.AutologState.AutoLogged;
		}

		public bool IsTemplateRecord { get => isTemplateRecord; set => isTemplateRecord = value; }
		bool isTemplateRecord;

		public ITemplateRecord TemplateRecord { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public BusinessObject InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord templateRecord)
		{
			throw new NotImplementedException();
		}

		public void LoadFromTemplateRecord(ITemplateRecord templateRecord)
		{
			throw new NotImplementedException();
		}

		public void SaveToTemplateRecord()
		{
			throw new NotImplementedException();
		}
	}
}
