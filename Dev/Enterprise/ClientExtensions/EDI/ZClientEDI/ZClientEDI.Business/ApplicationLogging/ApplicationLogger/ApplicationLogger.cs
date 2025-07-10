using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.ApplicationLogging.Business
{
	[CodeProperty(ApplicationLoggerSchema.Constants.ALG_Name), DescriptionProperty(ApplicationLoggerSchema.Constants.ALG_Description)]
	public class ApplicationLogger : AutoApplicationLogger, IAuditParent
	{
		public ApplicationLogger(
			BusinessObjectFactory factory,
			DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => Res.GetString("06C09C64-4385-451B-B745-72EC120D1634", "Application Logger");

		[MaxLength(35)]
		[List("Lookups.Products")]
		[ReadOnlyMember(nameof(IsInDatabase))]
		public override ZString ALG_Product { get => base.ALG_Product; set => base.ALG_Product = value; }

		[MaxLength(80)]
		[ReadOnlyMember(nameof(IsInDatabase))]
		public override ZString ALG_Name { get => base.ALG_Name; set => base.ALG_Name = value; }

		[MaxLength(256)]
		public override ZString ALG_Description { get => base.ALG_Description; set => base.ALG_Description = value; }

		[MaxLength(35)]
		public override ZString ALG_Category { get => base.ALG_Category; set => base.ALG_Category = value; }

		#region IAuditParent Members

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(ApplicationActiveLoggerSchema.AAL_ALG_ApplicationLogger, ApplicationActiveLoggerSchema.AAL_Environment);
			}
		}

		#endregion
	}
}
