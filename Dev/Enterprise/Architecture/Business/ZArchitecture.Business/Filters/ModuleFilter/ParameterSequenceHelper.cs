using System.Collections.Generic;
using System.Linq;

namespace Enterprise.ZArchitecture.Business
{
	public interface IParameterSequenceHelperRequired
	{
		ParameterSequenceHelper GetParameterSequenceHelper();
		void SetParameterSequenceHelper(ParameterSequenceHelper sequenceHelper);
	}

	public class ParameterSequenceHelper
	{
		readonly Stack<int> sequence;

		public ParameterSequenceHelper()
		{
			sequence = new Stack<int>();
		}

		public void BeginSequence()
		{
			sequence.Push(0);
		}

		public void EndSequence()
		{
			sequence.Pop();
		}

		public string NextSequence()
		{
			if (sequence.Count == 0)
			{
				BeginSequence();
			}
			var sequenceString = string.Join("_", sequence.Reverse());
			int lastElement = sequence.Pop();
			sequence.Push(lastElement + 1);

			return sequenceString;
		}
	}
}
