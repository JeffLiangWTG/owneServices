using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Macros;
using CargoWise.Macros.Testing;

namespace Enterprise.DocumentVisualizer.Testing
{
	abstract class DynamicDataTestCase : TestCaseWithMacros
	{
		protected void AssertNoErrors(IMacroExpression expr)
		{
			AssertMultilineASCIIEquals("expression evaluated with no errors",
				string.Empty,
				string.Join("\r\n", expr.Errors.Select(err => err.Message)));
		}

		#region Nested Types

		protected class Consol
		{
			public string Number { get; set; }
			public Organization Consignee { get; set; }
			public Organization Shipper { get; set; }
		}

		protected class Organization
		{
			public string Code { get; set; }
			public string Name { get; set; }
			public bool IsActive { get; set; }

			public List<Contact> Staff
			{
				get { return staff; }
			}

			readonly List<Contact> staff = new List<Contact>();
		}

		protected class Contact : INotifyPropertyChanged
		{
			public string Name
			{
				get { return name; }
				set
				{
					if (name != value)
					{
						name = value;
						NotifyChanged("Name");
					}
				}
			}

			string name;

			public int Number
			{
				get { return number; }
				set
				{
					if (number != value)
					{
						number = value;
						NotifyChanged("Number");
					}
				}
			}

			int number;

			public Car Car
			{
				get { return car; }
				set
				{
					if (car != value)
					{
						car = value;
						NotifyChanged("Car");
					}
				}
			}

			Car car;

			public Gender Gender
			{
				get { return gender; }
				set
				{
					if (gender != value)
					{
						gender = value;
						NotifyChanged("Gender");
					}
				}
			}

			Gender gender;

			public event PropertyChangedEventHandler PropertyChanged;

			void NotifyChanged(string name)
			{
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(name));
				}
			}
		}

		protected enum Gender
		{
			Unknown,
			Male,
			Female,
			Undecided
		}

		protected class Car : INotifyPropertyChanged
		{
			public string Make
			{
				get { return make; }
				set
				{
					if (make != value)
					{
						make = value;
						NotifyChanged("Make");
						NotifyChanged("Description");
					}
				}
			}

			string make;

			public int Year
			{
				get { return year; }
				set
				{
					if (year != value)
					{
						year = value;
						NotifyChanged("Year");
						NotifyChanged("Description");
					}
				}
			}

			int year;

			public string Description
			{
				get { return string.Format("{0} {1}", Year, Make); }
			}

			public event PropertyChangedEventHandler PropertyChanged
			{
				add { propertyChangedHandler += value; }
				remove { propertyChangedHandler -= value; }
			}

			internal PropertyChangedEventHandler propertyChangedHandler;

			void NotifyChanged(string name)
			{
				if (propertyChangedHandler != null)
				{
					propertyChangedHandler(this, new PropertyChangedEventArgs(name));
				}
			}
		}

		#endregion
	}
}