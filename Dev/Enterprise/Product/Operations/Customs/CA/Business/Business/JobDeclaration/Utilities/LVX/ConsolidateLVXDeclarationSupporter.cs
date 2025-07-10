using CargoWise.Common;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ConsolidateLVXDeclarationSupporter : Integration.Customs.CA.IConsolidateLVXDeclarationSupporter
	{
		public ConsolidateLVXDeclarationSupporter(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}
		readonly JobDeclaration declaration;

		#region IConsolidateLVXDeclarationSupporter Members

		public ZBool SupportConsolidateLVXDeclaration
		{
			get { return declaration.JE_MessageType == JobMessageTypeList.Codes.LVSForConsolidation; }
		}

		public ZString NotSupportConsolidateLVXDeclarationReason
		{
			get
			{
				return SupportConsolidateLVXDeclaration ? string.Empty : Res.GetString("0263d846-1345-41a8-b704-cdcd6aaa5048", "Trigger action Consolidate LVX Declaration is valid only for LVX declaration.");
			}
		}

		public CargoWise.EntityFramework.IProcessor CreateConsolidateLVXDeclarationProcessor()
		{
			return new ConsolidateLVXDeclarationProcessor(declaration);
		}

		#endregion
	}
}
