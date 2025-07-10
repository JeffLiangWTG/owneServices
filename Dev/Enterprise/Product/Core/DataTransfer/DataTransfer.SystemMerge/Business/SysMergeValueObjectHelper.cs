using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.DataTransfer.SystemMerge.Business.Res;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge
{
	public static class SysMergeValueObjectHelper
	{
		internal static void ImportCPDecAnswers(BusinessObjectFactory factory, Xsd.CPDecAnswerCollection xsdAnswers, ZGuid parentID, string parentTablePrefix)
		{
			foreach (Xsd.CPDecAnswer xsdAnswer in xsdAnswers)
			{
				var answerPk = new ZGuid(xsdAnswer.PK);
				var answer = factory.NewWithPrimaryKey<BaseCusEntryCPDec>(answerPk.ToGuid());

				answer.ON_ParentID = parentID;
				answer.ON_ParentTableCode = parentTablePrefix;
				answer.ON_CPDecNum = xsdAnswer.CPDecNum;
				answer.ON_AnswerCode = xsdAnswer.AnswerCode;
				answer.ON_Permit = xsdAnswer.Permit;
			}
		}

		internal static void ExportCPDecAnswers(BusinessObjectFactory factory, Xsd.CPDecAnswerCollection answerCollection, ZGuid parentID, string parentTablePrefix)
		{
			var query = new ZQuery(CusEntryCPDecSchema.ON_ParentID, parentID);
			query.AddToFilter(CusEntryCPDecSchema.ON_ParentTableCode, parentTablePrefix);
			BaseCusEntryCPDec[] answers = factory.Load<BaseCusEntryCPDec>(query);

			foreach (BaseCusEntryCPDec answer in answers)
			{
				Xsd.CPDecAnswer xsdAnswer = answerCollection.AddNew();

				xsdAnswer.PK = answer.PK.ToString();
				xsdAnswer.CPDecNum = answer.ON_CPDecNum;
				xsdAnswer.AnswerCode = answer.ON_AnswerCode;
				xsdAnswer.Permit = answer.ON_Permit;
			}
		}

		internal static InfoNotification GetImportedSuccessfullyNotification(string objectDescription)
		{
			return new InfoNotification(Res.GetString("816f1a0e-a844-48a5-b759-9da2ba23ac96", "{0} imported successfully", objectDescription) + "\r\n");
		}
	}
}
