import { Directive, ElementRef, HostListener } from '@angular/core';

@Directive({
  selector: '[editAutoSize]'
})
export class EditAutoSizeDirective {
	constructor(private el: ElementRef){

	}

	@HostListener('input') input() {
		this.autoGrow();
	}

	@HostListener('focus') onFocus() {
		this.autoGrow();
	}

	autoGrow() {
		this.el.nativeElement.style.height = "0px";
		this.el.nativeElement.style.height = (this.el.nativeElement.scrollHeight + 25)+"px";
	}
}