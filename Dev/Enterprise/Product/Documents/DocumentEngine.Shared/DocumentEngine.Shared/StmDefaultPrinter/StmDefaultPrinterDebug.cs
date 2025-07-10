#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Shared
{
	public partial class StmDefaultPrinter
	{
		#region SDP_SubjectTableCode

		[BusinessObjectTestExclude]
		public override ZString SDP_SubjectTableCode
		{
			get { return base.SDP_SubjectTableCode; }
			set
			{
				base.SDP_SubjectTableCode = value;

				if (!ValidSubjectTables.Any(o => ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(o.TableName) == SDP_SubjectTableCode))
				{
					throw new NotSupportedException(
						"You must add your table to 'ValidSubjectTables' for it to be a valid Parent for a DefaultPrinter." + "\r\n" +
						"This is to ensure that you delete the Default Printer record when the Parent is deleted.");
				}
			}
		}

		#endregion

		#region ValidSubjectTables

		public static IEnumerable<ITableSchema> ValidSubjectTables
		{
			get
			{
				return new ITableSchema[]
				{
					WhsAreaSchema.Instance,
					WhsWarehouseSchema.Instance,
					GlbStaffSchema.Instance,
					PkgPackageSchema.Instance
				};
			}
		}

		#endregion

		#region FillWithValidTestData

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			SDP_SubjectTableCode = "GS";
			SDP_SubjectID = Factory.New<DummyBusinessObject>().PK;

			var server = ((IBusinessObjectFactoryInternals)Factory).RowFactory.New(StmPrintServerSchema.Constants.TableName);
			var serverPK = Guid.NewGuid();
			server[StmPrintServerSchema.Constants.PK] = serverPK;
			server[StmPrintServerSchema.Constants.SPS_ServerName] = "Server";
			server.Table.Rows.Add(server);

			var printer = ((IBusinessObjectFactoryInternals)Factory).RowFactory.New(StmPrintQueueSchema.Constants.TableName);
			var pk = Guid.NewGuid();
			printer[StmPrintQueueSchema.Constants.PK] = pk;
			printer[StmPrintQueueSchema.Constants.SQ_SPS_Server] = serverPK;
			printer.Table.Rows.Add(printer);
			SDP_SQ_Printer = pk;

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

		#endregion
	}
}
#endif
