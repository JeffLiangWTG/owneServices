namespace Enterprise.ZArchitecture.Core
{
	public struct ReturnResult
	{
		string message;
		bool success;

		public string Message
		{
			get { return message; }
			set { message = value; }
		}

		public bool Success
		{
			get { return success; }
			set { success = value; }
		}
	}

	public struct ReturnResult<T>
	{
		string message;
		bool success;
		T value;

		public string Message
		{
			get { return message; }
			set { message = value; }
		}

		public bool Success
		{
			get { return success; }
			set { success = value; }
		}

		public T Value
		{
			get { return value; }
			set { this.value = value; }
		}
	}
}
