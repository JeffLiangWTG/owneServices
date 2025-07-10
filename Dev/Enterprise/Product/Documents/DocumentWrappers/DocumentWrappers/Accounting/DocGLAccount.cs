using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocGLAccount : DocBaseWrapper
	{
		DocGLAccount(AccGLHeader accGLHeader, BusinessObjectFactory factoryToWrap)
			: base(accGLHeader, factoryToWrap)
		{
		}

		public static DocGLAccount New(AccGLHeader accGLHeader, BusinessObjectFactory factoryToWrap)
		{
			if (accGLHeader == null)
			{
				return null;
			}
			else
			{
				return new DocGLAccount(accGLHeader, factoryToWrap);
			}
		}

		AccGLHeader AccGLHeader
		{
			get { return (AccGLHeader)WrappedObject; }
		}

		public override string ToString()
		{
			return AccountNumber;
		}

		public ZString AccountNumber
		{
			get { return AccGLHeader.AG_AccountNum; }
		}

		public ZString AccountType
		{
			get { return AccGLHeader.AG_AccountType; }
		}

		public DocGLAccount AlternateGLAccount
		{
			get { return DocGLAccount.New(AccGLHeader.AlternateNum, Factory); }
		}

		public DocGLAccount ConsolidationGLAccount
		{
			get { return DocGLAccount.New(AccGLHeader.ConsolidationNum, Factory); }
		}

		public DocGLAccount PercentOfGLAccount
		{
			get { return DocGLAccount.New(AccGLHeader.PercentNum, Factory); }
		}

		public ZString Column
		{
			get { return AccGLHeader.AG_Column; }
		}

		public ZBool ControlAccount
		{
			get { return AccGLHeader.AG_ControlAccount; }
		}

		public ZString DebitCredit
		{
			get { return AccGLHeader.AG_DebitCredit; }
		}

		public ZString Description
		{
			get { return AccGLHeader.AG_DescriptionMultilingual; }
		}

		public ZBool IsActive
		{
			get { return AccGLHeader.AG_IsActive; }
		}

		public ZString Notes
		{
			get { return AccGLHeader.AG_Notes; }
		}

		public ZInt PrintSequence
		{
			get { return AccGLHeader.AG_PrintSequence; }
		}

		public ZInt TotalLevel
		{
			get { return AccGLHeader.AG_TotalLevel; }
		}

		public ZString StatisticalUnits => AccGLHeader.AG_StatisticalUnits;

		protected override ZString DocManagerUniqueID
		{
			get { return AccountNumber; }
		}
	}
}
