using CargoWise.Common;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class SubmitAVSQuerySupporter : Integration.Customs.CA.ISubmitAVSQuerySupporter
	{
		public SubmitAVSQuerySupporter(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}
		readonly JobDeclaration declaration;

		#region ISubmitAVSQuerySupporter Members

		public bool SupportSubmitAVSQuery
		{
			get { return declaration.JE_MessageType == JobMessageTypeList.Codes.Import; }
		}

		public string NotSupportSubmitAVSQueryReason
		{
			get
			{
				return SupportSubmitAVSQuery ? string.Empty : Res.GetString("e16a2fff-9d0c-48d1-ae7c-68de68f59f28", "Trigger action Submit AVS Query is valid only for IMP declaration.");
			}
		}

		public CargoWise.EntityFramework.IProcessor CreateSubmitAVSQueryProcessor()
		{
			return new SubmitAVSQueryProcessor(declaration);
		}

		#endregion
	}
}
