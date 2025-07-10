using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class PersonalItemDecQuestionCollection : CusCodeDataCollection<PersonalItemDecQuestion>
	{
		public PersonalItemDecQuestionCollection(JobDeclaration declaration)
			: base(declaration, CusCodeDataTypeList.Codes.PersonalItemDecQuestion)
		{
		}
	}
}
