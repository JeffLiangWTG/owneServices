using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public static class BMRibbonResources
	{
		static readonly string BaseSourcePath = "_content/Enterprise.BufferManagement.NetworkVisualisation.GUI/images/";

		public static Dictionary<string, string> Icons
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ (NoResString)"Approve", $"{BaseSourcePath}Approve.svg" },
					{ "ArrowLeft", $"{BaseSourcePath}ArrowLeft.svg" },
					{ "ArrowRight", $"{BaseSourcePath}ArrowRight.svg" },
					{ (NoResString)"Bubble", $"{BaseSourcePath}Bubble.svg" },
					{ (NoResString)"Buffer", $"{BaseSourcePath}Buffer.svg" },
					{ (NoResString)"Copy", $"{BaseSourcePath}Copy.svg" },
					{ "ExtendedDiagram", $"{BaseSourcePath}ExtendedDiagram.svg" },
					{ (NoResString)"Job", $"{BaseSourcePath}Job.svg" },
					{ (NoResString)"Pin", $"{BaseSourcePath}Pin.svg" },
					{ (NoResString)"Scale", $"{BaseSourcePath}Scale.svg" },
					{ (NoResString)"Shape", $"{BaseSourcePath}Shape.svg" },
					{ "ShapeFromLink", $"{BaseSourcePath}ShapeFromLink.svg" },
					{ "ShapeStack", $"{BaseSourcePath}ShapeStack.svg" },
					{ (NoResString)"Status", $"{BaseSourcePath}Status.svg" },
					{ "Statuses_ASN", $"{BaseSourcePath}Statuses_ASN.svg" },
					{ "Statuses_CAN", $"{BaseSourcePath}Statuses_CAN.svg" },
					{ "Statuses_CLS", $"{BaseSourcePath}Statuses_CLS.svg" },
					{ "Statuses_OPN", $"{BaseSourcePath}Statuses_OPN.svg" },
					{ "Statuses_SUS", $"{BaseSourcePath}Statuses_SUS.svg" },
					{ "Statuses_WRK", $"{BaseSourcePath}Statuses_WRK.svg" },
					{ (NoResString)"Unapprove", $"{BaseSourcePath}Unapprove.svg" },
					{ (NoResString)"Unpin", $"{BaseSourcePath}Unpin.svg" },
					{ (NoResString)"Workflow", $"{BaseSourcePath}Workflow.svg" },
					{ (NoResString)"BottomPanelClose", $"{BaseSourcePath}BottomPanelClose.svg" },
					{ (NoResString)"LeftPanelClose", $"{BaseSourcePath}LeftPanelClose.svg" },
					{ (NoResString)"ChannelView", $"{BaseSourcePath}ChannelView.svg" },
				};
			}
		}

		public static Dictionary<string, string> IconsInBase64
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ "Statuses_ASN/base64", (NoResString)"iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAABDlBMVEUAAAD///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////+OjpOPj5STk5iTk5mUlJmfn6SgoKSgoKWqqq2qqq+rq666ur27u768vL28vL+8vMC9vcC9vcG+vsC+vsG/v8Do6Onp6enp6erp6evq6uv4+Pj5+fn6+vr7+/v8/Pz9/f3+/v7////X+ujZAAAAOHRSTlMABwgJDA0OFRgZGyIlJi0uMVVWV1hZgYaHioyWl5iZmpucncHGx8jKzNDT4uPl6err8/T1+Pn6+9xMCtgAAAABYktHRFmasvQYAAABhklEQVQ4y4WT61LCMBSEw01RUFQuKshVCipItdjSgogoigiKQhvk/V/ENDlpG8Bxf3Sm2a+bSboHIUdbyUJV1jS5mk+E0Lr2Ss2lo2YxumIH0q2lIO3U7/WjV8s1STuufyCztR8Qe6vHnO9lwXaQepj5/ktuL0AcuQhQIA0+cTAVQ+zVFD1fi/v4a9Rrt3vjKebErb1JCXxsvhg6lTGYYyAy5P6aFCB+X3fUpwQxlBBKMn+BB7pHQ7oLseKoAAFTwwsY3xCRQ1UIGOmCxhAhoRsWYD2IQNdiEQ2kAtARgQ4Ad38B9w7w7xZVAN5EYAKAhPIAfIjHnAKQRQk4pvksXJQJxzxEQQUiZo+u/2RCgBJEqAgR1uyV/6yh7VMgb/dJYxHYMqfv3U6nO/kkPgtQI3YhTuF3E8I053PyYD4BTljlJF4YbFFhXpiyj5UyXNtcuetdXutYfVNpa/vuYGyfr9e+HPaOli+liIOjHvtWpjOcUdyxU3KRDfMdjGcrDVVtVM6Ogu7qL87B8KIh682LAAAAAElFTkSuQmCC" },
					{ "Statuses_CAN/base64", (NoResString)"iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAABOFBMVEUAAAD/SUn/QED/OTn/QED/Ozv/Nzf/PT3/QDX/PT32Qjn4PDz4Pjf4PDb5Pjn5PTf6Pjn5Pzn5Pjj5Pjj5QDr5Pzn5Pzn5Pzn5Pjn5Pzn6QDj6Pzr6Pjn6QDn6Pzn6Pzj6Pzr6QDn6Pzn6Pzn6Pzn6Pzj6Pjn6Pzn6Pzr6Pzj6Pzn6Pzr7Pzn7Pzn6Pzn6Pzn6Pzr6Pzn6Pzn6Pzn6Pzn6Pzn6Pzn6PznzPTf0Pjj1Pjj2Pjj3Pjj4Pzn4WVT4W1b4XFf4Xln4X1r4YFv5Pzn5WFP5WVT5WlX5W1b5XFf5XVj5Xln5YFv6Pzn6WFP6WlX6W1b6XFf6YFv7jor7j4v7kY37ko77mJT8j4v8kY78n5z8paL9wL79wb/9wsD9w8H+6Of+6un+6+r+6+v/7Oz/7u3/7u7///88NnanAAAAOHRSTlMABwgJDA0OFRgZGyIlJi0uMVVWV1hZgYaHioyWl5iZmpucncHGx8jKzNDT4uPl6err8/T1+Pn6+9xMCtgAAAABYktHRGdb0+mzAAABkklEQVQ4y4WT51bCQBCFFwFFQVEpCkiVooKgwVDCxq7YomDDBiIief83MNmZFMo53F97cr/M7s7eIUTXrC+WKZRKhUzUayfjWkwUJV3FuGvEtgbL0pBKgRmz79qRxpSaN/zlgjRBObf+P/PPh8QIB/gz22M2IltWBgTRF01Cws/uV2a+KL51mpTpofPOEEnaVzdJsAKi2JLlv8eaovuuLH+qhGKElP4VocCzrKh/Iwj1nrp6hRKcnfiwwJfMiNsr5ssdLOEhMQQaP0CA/9ukAERIBgBau+jKugZ3NQRSZE8DhMuu4QsakCe8DlQPkRicVXXgYDowdYuph4wi0OiZr9nXrhkmXgSwUafHQHwjsEJsHDQKWn1SqRxBqwHgbITEAaAtdeuqIvUwHxRaHVXzVGKPRelT+1pgqrdfKGWPxTvVQASwBKU1lLKEAusQuZSEeaIozJSUtEAoHdnJkdtd0GLtzk0KbXbJGIy5zfHYJx3m0bL4ueGp4dcsI9PpCJkQLuKcMN82Tzid5/l8emPVZnz9B4da6BWHlSODAAAAAElFTkSuQmCC" },
					{ "Statuses_CLS/base64", (NoResString)"iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAABFFBMVEUAAACSkpKAgJ+Ojo6VlZWJiYmSkpKSkpKKipWPj4+OjpePj5aRkZGNjZSOjpOQkJaNjZKNjZOOjpSNjZOOjpSPj5KOjpKPj5OOjpOOjpSOjpSPj5KOjpOPj5SOjpOOjpONjZKOjpONjZSNjZOOjpOOjpOOjpONjZKPj5SOjpONjZOOjpOOjpOPj5OOjpOOjpOOjpKOjpOOjpOOjpOOjpOOjpOOjpOOjpOLi5CMjJGNjZKOjpOPj5SSkpeUlJmVlZmVlZqfn6SgoKShoaWioqajo6ewsLOysrWysra1tbm4uLvMzM7OztDPz9Hc3N7h4ePl5ebm5ufx8fH09PX19fX19fb29vb29vf4+Pn9/f3+/v7///+N5y15AAAAOHRSTlMABwgJDA0OFRgZGyIlJi0uMVVWV1hZgYaHioyWl5iZmpucncHGx8jKzNDT4uPl6err8/T1+Pn6+9xMCtgAAAABYktHRFt0vJU0AAABcUlEQVQ4y3WT2VrCMBBGAxStgqKyKCCyyKKCRYsthLjjrrjhSt7/PWwyTdqG+l/16zmdTJMJQjJz2Uqra9vdVjkTR7NZqvWITK+aVHCs0CeB2Pmonyf3yEwaCx5f6ZKQdFLy+1DuGDrw6K4ChjzOw06MC4UwDEqO/1//H+4Yh2yRWkj1x58HKFF09q+nfonJmNIpGGYcZdW6nFP6e85LpFElyDEeck5fj7mwhVqucPV8g1lc/naGudBAB8Cvv+n03uHkCfgphi4NZHHh5JMtezcgY8Fd4UgIE97YrcdhBUeAJfDoi8oIzgSDNcmbv5x4fCC502QZmsT44iOEkxLKCME1JAdhFWmmMAbMULipIVQlssbo/UX0B5yU2TzZ7l5jNz5uJdhA5OVpSOxysgEj11DOU2BSj8BQ6m111CD7i2KsU53gsELay97FmN+eHfq67r9akZwZxNZ6RLmdetGnmFuJkPutpUtNw7KM5uaa5r39A4uLwlH56WDjAAAAAElFTkSuQmCC" },
					{ "Statuses_OPN/base64", (NoResString)"iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAABQVBMVEUAAAD///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////+OjpOWlpqWlpuYmJ2ZmZ2ZmZ6amp6goKShoaWhoaatrbGurrGurrKvr7KysrWysrazs7azs7e9vcDAwMPBwcPBwcTCwsXMzM7MzM/Nzc/NzdDOztDQ0NLR0dPS0tTY2NrZ2dvh4eLh4ePi4uPo6Onp6erq6uvr6+zw8PHx8fHx8fLy8vLy8vP09PX19fX19fb8/Pz9/f3///+5OhfxAAAAOHRSTlMABwgJDA0OFRgZGyIlJi0uMVVWV1hZgYaHioyWl5iZmpucncHGx8jKzNDT4uPl6err8/T1+Pn6+9xMCtgAAAABYktHRGolYpUOAAABnElEQVQYGXXBi1YSURiA0R9Eo8SsvJSaoiZSSWCDg+OEZeQloMEQEAKlouTyvf8DNJ4za5IF7i3/TS2sx1OZTCoenZ+UUTObaXzpjYgMm1jeZ0hmKSh3RN4yIvZQfE9SjJGYFU8kxViJsCjBN9zj9YTcWsZ3Uy+X6118i+Ka2cdzlbPz34p5O3eN531YRDbxXHy8HOAa1A4reFZEptJoF5//4ukcVdCMSVlAuzr8g6+TbaPNyTpa7hJXs1Rq4aodo61JHOXGHgBFy+UAfbuLEpNdlHoeaFrKD+CsgZIUE+XcAUqWUgCc7yh7YqKcO0DJUgpAsYyyJ7so9a9Ay1JawGkDJSlxlK49ABzL5QD9gx5KTKJoX2q4moVCC1f1BG1V5tGuDzv4fmd/oj2VkIFWOerg+fWpimaERDbwVLK1Pq5+9UMVT1REIhk87WP7rFg8PThp4zGnxbWEr9colxs9fC/kVjDGPbYCooR3GOvdI/HMJhhj57H4HrxixFZY7ggsGgwxnwdkWHjFwGesTcuo0NzqdtI0k9svn4XE9w+NzyseoBE4TQAAAABJRU5ErkJggg==" },
					{ "Statuses_SUS/base64", (NoResString)"iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAA21BMVEUAAAD/tiT/vyD/xhz/vxX/xBT/yBL/whj0vxX1whT2xhz4wxf4wRz4wxv5wRf5whz6wRr5wxj5xBj5xBr5whr5wxr5xBr5whn5wxn5xBr6wxr4xBr4whn4wxn4wxn4wxn4xBn4wxn5wxj4xBn5whj5wxr5wxr5whn5wxn5wxn5wxn5wxn5wxn5wxj6wxn4wxn4wxn5wxn5wxn5wxn5wxn5wxn5wxr5wxnxvRjyvhjzvhj0vxn1wBn2wRn3wRn4whn5wxn87sD978L978P+9dj++ef++ej++en///+WAYCvAAAAOHRSTlMABwgJDA0OFRgZGyIlJi0uMVVWV1hZgYaHioyWl5iZmpucncHGx8jKzNDT4uPl6err8/T1+Pn6+9xMCtgAAAABYktHREjwAtTqAAABVUlEQVQ4y4WTa1uCQBCFR8WitKy8lJqiJlJpYBiosItrafn/f1HszoYoSOfL8sx52esZgEhn9Z5hua5laLUiJHU1mLFIs375yC605uxAbjMf98uvLCH9Yu/fWCxFk0r0P/qrmJBQ0c+/HLkR81wQQEv6QUySaIjzzYUfVmkkRBh754sMxARB8LML9UnIFx+/OREa7fD+ZuhTXt9tfH8jPigSdhHqOAGVgOdJgOIUVehJgCCwXCJAJNAFIxvQYcqBICA+AosFAj4JNxFaJjgCoEmACuDjf2CaDZi4ydOADlr2JjtQyz7mLSh2FmArAH151VteX3vemo9bedUaz5OLj0UJ8aUIofhYTokHoimfkyMoKn32gJHTTwVmmMNQquP0yL1d/sW6MkkL7fh63xjnT8nYD9V4a+Ua9mHXOPe5o+5U2zHE7pZS+lupdkam45ijxztlX/0FYpzGvlIrsbIAAAAASUVORK5CYII=" },
					{ "Statuses_WRK/base64", (NoResString)"iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAABHVBMVEUAAABJ20lA32A541VA1VU72E4320k921U11VUz1lI52VU10lM31lM211E5108301M51VM21VE41VM41lI31lE21FM31VE31VI31VE31VE31VI21VI41VM31VI31VI31lM21FI41VI31VE31VI31VI31VI31VI41VI31VM31VI41VI31VI31VI31VI31VI21VI31VI31VI31VI31VI21VI31VI31VI31VI20FA20VA20lE301E31FI31VI51FM51VQ601Q61FQ61FU61VRD1lxD11xM2GRN2GRd3HNe3HNl3npz4YZ04IaE5JWF5ZWI5Zii6q6i66+378C378HK89HK9NLb9+Df+OPg+OTi+ebr++3r++73/fj9//7///8pWt8bAAAAOHRSTlMABwgJDA0OFRgZGyIlJi0uMVVWV1hZgYaHioyWl5iZmpucncHGx8jKzNDT4uPl6err8/T1+Pn6+9xMCtgAAAABYktHRF4E1mG7AAABcklEQVQ4y3WT6UKCQBSFR8WitKxcSs010UpDw8CckdRs32wvS+f9HyNwNkA6Pzkfd+6dewYArqVksdY0jGatkAiDRa2V2yZXuxT12KFMx3TJSAedfvTQXJCyIvyNpumjRoz/7+tbhEz84AH90LPkIvZDcyAj7J6HSc3n63AfIeRGju1DyszvT576iDKMyFr316Y+GmH8dQ8hQSighUGSFkBoiC193NiIKBIHRQbAcxvAs9crghAgD2oeAOPpyyXkpyigRQAkAIx/xiNWQgU6BwZY6POOljjhAPQAiAEtH+B3PGRtqqxJxxHT5wvIu1RAwdPk7O2665gzBxLuMd9vTy1fjLkJJM0BfD90u3OfFdAkAErsqs8mj0MIoeuqC3aeDLosBKktlqVH7ECk2TqpHOveIZFT/gtMJUBCKdf9I3e0ymIda/iFtr4uHsby3mLoK7LzaQVSmtvWtwOe1ylnHYiWj/i8bymeq6q6rlZ3tyTx9Q+bGsgC2WRz0gAAAABJRU5ErkJggg==" },
				};
			}
		}
	}
}
