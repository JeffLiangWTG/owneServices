using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class DeclarationBatchListenerTest : JobBatchListenerTestCase
	{
		protected override NavisionBatchListener BatchListener
		{
			get
			{
				return new DeclarationBatchListenerTestClass();
			}
		}

		protected override Type BusinessObjectCollectionType
		{
			get
			{
				return typeof(BaseJobDeclarationCollection);
			}
		}

		protected override string BusinessObjectTableName
		{
			get
			{
				return JobDeclarationSchema.Constants.TableName;
			}
		}

		protected override BusinessObject BusinessObjectForTesting()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			return declaration;
		}

		protected override Type BusinessObjectType
		{
			get
			{
				return typeof(BaseJobDeclaration);
			}
		}

		protected override Type ExporterType
		{
			get
			{
				return typeof(DeclarationFlatFileExporter);
			}
		}

		class DeclarationBatchListenerTestClass : DeclarationBatchListener, NavisionBatchListenerTest.IBatchListenerTestClass
		{
			public DeclarationBatchListenerTestClass() : base(ZDateTime.Now)
			{
			}

			public new void Process(BusinessObject matchingBusinessObject, StmALog log, INotifications notifications)
			{
				base.Process(matchingBusinessObject, null, notifications);
			}

			public Type BusinessObjectCollectionTypeExposed
			{
				get
				{
					return base.BusinessObjectCollectionType;
				}
			}

			public NavisionFlatFileExporter ExporterExposed
			{
				get
				{
					return base.Exporter;
				}
			}
		}
	}
}
