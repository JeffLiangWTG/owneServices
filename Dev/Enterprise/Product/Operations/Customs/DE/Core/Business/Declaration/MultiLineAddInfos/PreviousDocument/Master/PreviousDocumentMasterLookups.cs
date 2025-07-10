using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class PreviousDocumentMasterLookups : ZLookups
	{
		public PreviousDocumentMasterLookups(PreviousDocumentMaster parent)
			: base(parent)
		{
		}

		new PreviousDocumentMaster Parent => (PreviousDocumentMaster)base.Parent;

		public CodeDescriptionPairList ProcedureList => Factory.GetCachedValue("DE.PreviousDocumentLookups.ProcedureList|" + Parent.IsImport, () => PreviousDocumentLookups.GetProcedureList(Parent.IsImport));

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList
		{
			get
			{
				var attributeFilterList = new List<RefCusCodeListAttributeFilter>() { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, JoinCondition.And, false, null, "True") };// param value
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Germany, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice }, ZDateTime.Now, attributeFilterList, false);
			}
		}

		public CodeDescriptionPairList AuthorizationNumberList
		{
			get
			{
				var transactionDate = ZDate.Today;
				var procedure = Parent.CSI_Procedure;
				var parentProvider = Parent.Parent;
				var jobDeclaration = parentProvider.JobDeclaration;
				CusEntryInstruction entryInstructionToObtainAuthorizations = null;
				if (jobDeclaration.IsImport && parentProvider is CusEntryInstruction entryInstruction)
				{
					entryInstructionToObtainAuthorizations = entryInstruction;
				}
				var result = jobDeclaration?.GetDeclarationAuthorizationNumberList(entryInstructionToObtainAuthorizations, transactionDate, procedure) ?? new CodeDescriptionPairList();
				return result;
			}
		}
	}
}
