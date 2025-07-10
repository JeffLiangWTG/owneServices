using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.EU.Business.OperationalActions
{
	public class DeclarationUpdateSupportingDocumentsApplicator : AutoDeclarationUpdateSupportingDocumentsApplicator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public DeclarationUpdateSupportingDocumentsApplicator(BusinessObjectFactory factory) : base("Update Supporting Documents", factory)
		{
		}

		#region override properties
		[List(nameof(DocumentCodeList))]
		public override ZString DocumentCode { get => base.DocumentCode; set => base.DocumentCode = value; }

		#endregion

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new UpdateSupportingDocumentsOperationalActionRunner();
			foreach (var bo in targets)
			{
				if (bo is JobDeclaration declaration)
				{
					runner.UpdateSupportingDocuments(this, declaration, log);
				}
			}
		}

		#region Lookups

		public ICollection DocumentCodeList
		{
			get
			{
				return Factory.GetSupportingDocumentList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, UniversalReferenceConstants.RefCusCodeListDirectionType.Both, UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			}
		}

		#endregion
	}
}
